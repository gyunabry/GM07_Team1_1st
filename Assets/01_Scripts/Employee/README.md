# 직원 공통 시스템 안내

## 파일 안내

| 파일 | 역할 |
| --- | --- |
| `EmployeeManager.cs` | 직원 건물 등록, 기본/추가 고용, 직원 제거, 공통 상태 변경을 관리합니다. |
| `EmployeeBuildingProfile.cs` | 매니저에서 건물 ID와 고용할 직원 데이터, 정원, 기본 자동 고용 수를 연결합니다. |
| `EmployeeRuntimeData.cs` | 직원별 고유 ID, 직군, 소속 건물, 공통 업무 상태를 보관합니다. |
| `EmployeeWorkState.cs` | `Idle`, `Moving`, `Working` 공통 업무 상태를 정의합니다. |

## 공통 데이터

직원의 정적 정의는 `Assets/01_Scripts/Data`에서 관리합니다.

| 파일 | 역할 |
| --- | --- |
| `EmployeeDataSO.cs` | 직원 ID, 표시 이름, 직군을 보관하는 공통 ScriptableObject입니다. |
| `EmployeeRole.cs` | 사냥꾼, 운반원, 계산원 직군을 정의합니다. |

## 씬 구성

게임플레이 씬에 빈 오브젝트를 만들고 `EmployeeManager`를 붙입니다.

```text
EmployeeManager
└─ Building Profiles
   ├─ Building Id             -- 직원 건물의 BuildingDataSO.BuildingId
   ├─ Employee Data           -- EmployeeDataSO 참조
   ├─ Max Employees           -- 해당 건물의 최대 직원 수, 기본값 3
   └─ Automatic Hire Count    -- 건물 등록 시 기본 고용 수, 기본값 1
```

- 직원 건물마다 `Building Profiles` 항목을 하나씩 등록합니다.
- `Building Id`는 건물 데이터의 `BuildingId`와 정확히 일치해야 합니다.
- `Employee Data`에는 해당 직원 직군의 `EmployeeDataSO`를 연결합니다.
- 등록되지 않은 건물은 직원 건물로 처리하지 않으며, 고용 API는 `false`를 반환합니다.
- 판매 직원 구성은 `Sales/README.md`를 참고합니다.

## 건물 시스템 연동

건물 시스템은 구매·배치 또는 세이브 로드가 완료된 뒤, 판매·파괴 직전에 아래 API를 호출합니다.

```csharp
// 건물 구매, 배치, 세이브 로드 완료 후
employeeManager.TryRegisterBuilding(placedBuilding);

// 건물 판매 또는 파괴 직전
employeeManager.TryUnregisterBuilding(placedBuilding);
```

- `TryRegisterBuilding`은 같은 건물의 중복 등록을 허용하지 않습니다.
- 등록에 성공하면 프로필의 `Automatic Hire Count`만큼 직원을 자동 고용합니다.
- `TryUnregisterBuilding`은 해당 건물에 소속된 모든 직원을 제거합니다.
- 일반 직원 건물 연동 호출은 건물 담당 영역에서 수행한다. 단, 판매대는 `SalesEmployeeCheckoutOperator`가 완성·파괴 생명주기에 맞춰 직접 처리한다.

## 고용 및 상태 처리

```csharp
// 정원 내 직원 1명 추가 고용
if (employeeManager.TryHireAdditional(placedBuilding, out EmployeeRuntimeData employee))
{
    employeeManager.TrySetWorkState(employee, EmployeeWorkState.Working);
}

// 특정 직원 제거
employeeManager.TryRemoveEmployee(employee);

// 건물 소속 직원 조회
if (employeeManager.TryGetEmployees(placedBuilding, out IReadOnlyList<EmployeeRuntimeData> employees))
{
    // 직원 목록 사용
}
```

- 추가 고용은 현재 무료이며, `Max Employees` 정원을 초과하면 실패합니다.
- 직원은 한 건물에만 소속되며, 각 런타임 직원은 고용 당시의 `EmployeeDataSO`를 참조합니다. 해제된 직원의 소속 건물은 `null`, 공통 상태는 `Idle`이 됩니다.
- 직군별 AI는 `EmployeeRuntimeData`를 직접 수정하지 않고 `TrySetWorkState`로 상태를 변경합니다.
- `EmployeeHired`, `EmployeeRemoved`, `EmployeeWorkStateChanged` 이벤트로 후속 UI·AI 시스템을 연결할 수 있습니다.

## 확인 체크리스트

- `EmployeeManager`가 게임플레이 씬에 하나만 존재합니다.
- 각 직원 건물의 `BuildingId`와 `Building Profiles` 설정 값이 일치합니다.
- 건물 구매·배치·로드 후 `TryRegisterBuilding`이 한 번 호출됩니다.
- 건물 판매·파괴 전 `TryUnregisterBuilding`이 호출됩니다.
- 기본 고용 수가 정원을 넘지 않고, 추가 고용이 정원까지만 성공합니다.
