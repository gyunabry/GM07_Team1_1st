using System;
using UnityEngine;

/// <summary>
/// EmployeeManager가 건물별 고용 규칙을 설정하는 구성 값입니다.
/// </summary>
[Serializable]
public sealed class EmployeeBuildingProfile
{
    [SerializeField] private string buildingId;
    [SerializeField] private EmployeeDataSO employeeData;
    [SerializeField, Min(1)] private int maxEmployees = 3;
    [SerializeField, Min(0)] private int automaticHireCount = 1;

    public string BuildingId => buildingId;
    public EmployeeDataSO EmployeeData => employeeData;
    public int MaxEmployees => Mathf.Max(1, maxEmployees);
    public int AutomaticHireCount => Mathf.Clamp(automaticHireCount, 0, MaxEmployees);
    public bool IsValid => !string.IsNullOrWhiteSpace(buildingId) && employeeData != null;
}
