/// <summary>
/// 고용된 직원 한 명의 공통 런타임 정보입니다.
/// 프리팹, 인벤토리, 능력치, 현재 작업 대상은 직군별 구현에서 확장합니다.
/// </summary>
public sealed class EmployeeRuntimeData
{
    public int EmployeeId { get; }
    public EmployeeDataSO Data { get; }
    public EmployeeRole Role => Data.Role;
    public PlacedBuilding AssignedBuilding { get; private set; }
    public EmployeeWorkState WorkState { get; private set; }
    public bool IsEmployed => AssignedBuilding != null;

    internal EmployeeRuntimeData(int employeeId, EmployeeDataSO data, PlacedBuilding assignedBuilding)
    {
        EmployeeId = employeeId;
        Data = data;
        AssignedBuilding = assignedBuilding;
        WorkState = EmployeeWorkState.Idle;
    }

    internal void SetWorkState(EmployeeWorkState workState)
    {
        WorkState = workState;
    }

    internal void Release()
    {
        AssignedBuilding = null;
        WorkState = EmployeeWorkState.Idle;
    }
}
