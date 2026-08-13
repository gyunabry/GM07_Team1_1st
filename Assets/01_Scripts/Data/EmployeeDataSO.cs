using UnityEngine;

/// <summary>
/// 동일 직군 직원이 공유하는 정적 공통 데이터입니다.
/// 직군별 능력치와 시각 프리팹은 후속 상세 직원 데이터에서 확장합니다.
/// </summary>
[CreateAssetMenu(fileName = "EmployeeData", menuName = "Tycoon/Employee Data")]
public sealed class EmployeeDataSO : ScriptableObject
{
    [field: SerializeField] public string EmployeeId { get; private set; }
    [field: SerializeField] public string EmployeeName { get; private set; }
    [field: SerializeField] public EmployeeRole Role { get; private set; }
}
