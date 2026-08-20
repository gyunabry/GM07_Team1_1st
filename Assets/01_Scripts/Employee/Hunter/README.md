# 사냥 직원

사냥 직원은 지정된 `SpawnArea` 안에서 몬스터를 처치하고 드롭 재료를 사냥터 전송기에 보관한다. 직원마다 독립적인 FSM과 소지함을 사용한다.

## 구성

| 파일 | 역할 |
| --- | --- |
| `HunterWorker.cs` | 개별 직원의 `Idle`, `Trace`, `Attack`, `Get`, `Store` FSM과 대상 예약을 처리한다. |
| `HunterCargo.cs` | 직원별 재료 소지량과 전송기 인벤토리 이관을 처리한다. |
| `HunterStatModifiers.cs` | 스킬·건물 업그레이드가 전달할 능력치 보정값 계약이다. |
| `HunterBuildingController.cs` | 건물의 고용 이벤트에 맞춰 사냥 직원 프리팹을 풀에서 대여·반환한다. |
| `HuntingTransmitter.cs` | 사냥터 전송기의 `ItemInventory`와 직원 납품 위치를 제공한다. |
| `HunterFleeState.cs` | 사냥 직원에게 피격된 몬스터가 공격자를 피해 이동하는 상태다. |
| `Assets/02_Prefabs/Employee/HunterEmployee.prefab` | 임시 캡슐 모델, `NavMeshAgent`, `HunterWorker`를 포함한 사냥 직원 프리팹이다. |

## 건물 설정

사냥 직원 건물 루트에 `HunterBuildingController`를 추가한다.

- `Hunter Prefab`: `Assets/02_Prefabs/Employee/HunterEmployee.prefab`을 연결한다.
- `Home Point`: 대기 위치. 비워 두면 건물 루트 위치를 사용한다.
- `Spawn Area`: 사냥 가능한 범위를 나타내는 `SpawnArea`의 Collider
- `Transmitter`: 해당 사냥터에만 연결되는 `HuntingTransmitter`

게임 씬의 `EmployeeManager > Building Profiles`에는 사냥 건물의 `BuildingId`, Hunter 역할 `EmployeeDataSO`, 최대 고용 수, 자동 고용 수를 설정한다. 건물 완성 시 컨트롤러가 건물을 등록하고, 고용 이벤트마다 직원을 풀에서 생성한다.

## FSM 우선순위

1. 소지량이 최대치면 `Store`
2. 획득 가능한 드롭이 있으면 `Get`
3. 현재 전투 대상이 공격 범위 안이면 `Attack`
4. 도달 가능한 미예약 몬스터가 있으면 `Trace`
5. 대상이 없으면 `Idle`

- `Trace`와 `Attack`은 같은 몬스터를 계속 유지한다.
- 몬스터와 드롭은 한 직원만 예약한다. 사냥 직원이 마지막 공격으로 몬스터를 처치하면, 처치 위치에 생성된 드롭을 먼저 예약한다.
- 완전한 NavMesh 경로가 없는 대상은 예약하지 않으며, 이동 중 경로가 무효화되면 예약을 해제하고 다시 판단한다.
- 대상이 없는 경우에만 `Target Search Interval` 간격으로 몬스터·드롭 전체 목록을 탐색한다. 현재 전투·획득 대상과 처치 직후 예약 드롭은 즉시 확인한다.
- 공격·획득 범위는 피벗 높이 차이의 영향을 받지 않도록 XZ 평면 거리로 판정한다.
- `Get` 완료 후 소지량에 여유가 있으면 즉시 다시 대상 선택을 수행한다.

## 기본 능력치

| 능력치 | 기본값 |
| --- | --- |
| 공격력 | 5 |
| 공격 범위 | 2m |
| 공격 간격 | 2초 |
| 이동 속도 | 5 |
| 최대 소지량 | 20 |

현재 공격 이펙트는 `HunterWorker > Attack Effect`에 프리팹을 연결하면 생성할 수 있다. 스킬 시스템은 `HunterWorker.SetStatModifiers(HunterStatModifiers)`를 호출해 이동 속도, 소지량, 공격력, 공격 범위 보정값을 즉시 반영할 수 있다. 이 호출 연결은 스킬 담당 시스템에서 구현한다.

## 전송기 연동

`HuntingTransmitter`의 `Inventory`가 사냥 직원의 납품 대상이다. 소지량이 가득 찬 직원은 전송기의 `Deposit Point`로 이동해 가능한 수량만 보관하고, 남은 공간이 생기면 다시 사냥한다. 전송기 공간이 없어 납품하지 못하면 전송기 앞에서 공통 업무 상태 `Idle`로 대기하며 납품을 재시도한다. 전송기 재고를 상점 전송기로 이송하는 기능은 전송기 담당 시스템에서 같은 `ItemInventory`를 사용해 연결한다.

건물 판매로 직원이 제거될 때도 먼저 이 전송기에 가능한 수량만 납품한다. 전송기 공간이 부족해 남은 화물은 폐기한 뒤 직원이 반환된다.
