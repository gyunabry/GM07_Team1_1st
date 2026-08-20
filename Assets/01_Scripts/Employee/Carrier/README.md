# 운반 직원

운반 직원은 HUD의 직원 UI에서 배정된 명령에 따라 상점 내부의 재료와 생산품을 운반한다. 명령이 없는 고용 직원은 운반 직원 건물 안에서 대기하는 것으로 처리하며, 프리팹은 화면에 표시하지 않는다.

## 구성

| 파일 | 역할 |
| --- | --- |
| `CarrierCommandService.cs` | HUD가 사용하는 전역 명령 API와 전송기 연동 값을 관리한다. |
| `CarrierEmployeeBuildingController.cs` | 운반 직원 건물의 자동 고용, 직원 프리팹 대여·반환을 관리한다. |
| `CarrierWorker.cs` | 운반 직원 한 명의 NavMesh 이동, 회수·납품, 대기 상태를 처리한다. |
| `CarrierCommand.cs` | 명령 종류와 대상 생산 건물을 보관한다. |
| `CarrierCommandType.cs` | `Material`, `Product` 명령 종류를 정의한다. |
| `Assets/02_Prefabs/Employee/CarrierEmployee.prefab` | 풀링하는 운반 직원 프리팹이다. |

명령의 실제 대상은 생산 아이템이 아니라 **생산 건물**이다. HUD에는 생산 건물이 선택한 아이템을 표시할 수 있지만, 같은 아이템을 생산해도 건물이 다르면 배정 직원은 섞이지 않는다.

## 건물 설정

운반 직원 건물 프리팹의 루트에 `CarrierEmployeeBuildingController`를 추가한다.

- `Carrier Employee Prefab`: `CarrierEmployee.prefab`을 연결한다.
- `Home Point`: 명령이 없는 직원이 대기할 직원 건물 앞 위치를 연결한다. 비워 두면 건물 루트 위치를 사용한다.
- 게임플레이 씬의 `EmployeeManager` `Building Profiles`에 운반 직원 건물의 `BuildingId`, `EmployeeDataSO(Carrier)`, 최대 고용 수, 자동 고용 수를 설정한다.

건물이 완성되면 컨트롤러가 `EmployeeManager`에 건물을 등록한다. 고용된 직원은 `PoolManager`에서 프리팹을 대여하며, 직원 제거 또는 건물 판매 시 풀에 반환한다.

## HUD 명령 API

`EmployeeManager`는 시작 시 `CarrierCommandService`를 생성한다. HUD는 이 서비스만 조회하고 운반 직원 건물 컨트롤러를 직접 참조하지 않는다.

```csharp
CarrierCommandService service = FindFirstObjectByType<CarrierCommandService>();

// + 버튼
service.TryAssignCommand(CarrierCommandType.Material, productionBuilding);
service.TryAssignCommand(CarrierCommandType.Product, productionBuilding);

// - 버튼
service.TryClearOneCommand(CarrierCommandType.Material, productionBuilding);
service.TryClearOneCommand(CarrierCommandType.Product, productionBuilding);

// UI 표시
int assignedCount = service.GetCommandCount(CarrierCommandType.Material, productionBuilding);
bool canAssign = service.GetAvailableWorkerCount() > 0;
```

- `+`는 대기 중인 운반 직원 한 명을 해당 생산 건물·명령에 배정하고, 직원 건물 출입 위치에서 프리팹을 활성화한다.
- `-`는 해당 생산 건물·명령에 배정된 직원 한 명을 해제한다.
- 대기 직원이 없으면 `+` 버튼을 비활성화한다.

## 전송기 연동

전송기 담당 코드는 공용 재료 인벤토리와 재료 회수 위치를 전역 서비스에 한 번 전달한다.

```csharp
service.ConfigureLogistics(transmitterInventory, transmitterWorkPoint);
```

- `transmitterInventory`: 상점 통합전송기의 재료 `ItemInventory`
- `transmitterWorkPoint`: 운반 직원이 재료가 없을 때 대기하고 회수·반납하는 위치

전송기 위치와 인벤토리는 운반 직원 건물 컨트롤러가 직접 찾지 않는다.

## 명령 동작

| 명령 | 이동 순서 | 아이템이 없을 때 |
| --- | --- | --- |
| `Material` | 통합전송기 → 지정 생산 건물 | 통합전송기 앞에서 대기 |
| `Product` | 지정 생산 건물 → 가장 가까운 판매대 | 생산 건물 앞에서 대기 |

- 재료 또는 생산품을 최대 운반량까지 회수한다.
- 1개 이상 회수한 뒤 추가 회수가 불가능하면 납품을 진행한다.
- 납품처가 가득 차면 납품처 앞에서 대기한다.
- 명령이 없으면 직원은 운반 직원 건물 안에서 대기하며 프리팹은 비활성화한다.
- `-`로 명령을 해제하면, 이미 소지한 물품만 현재 목적지에 납품한 뒤 직원 건물로 복귀해 프리팹을 비활성화한다.
- 대상 생산 건물의 생산품이 변경되면 그 건물의 `Material`·`Product` 명령이 초기화된다. 재료는 통합전송기에 반납하고, 생산품은 가장 가까운 판매대에 납품한 뒤 복귀해 프리팹을 비활성화한다.
