using System;
using UnityEngine;

/// <summary>
/// EmployeeManager가 건물�?고용 규칙???�정?�는 구성 값입?�다.
/// </summary>
[Serializable]
public sealed class EmployeeBuildingProfile
{
    [SerializeField] private string buildingId;
    [SerializeField] private EmployeeDataSO employeeData;
    [SerializeField, Min(1)] private int maxEmployees = 3;
    [SerializeField, Min(0)] private int automaticHireCount = 1;
    [SerializeField, Min(0)] private int[] additionalHireCosts = { 100, 200, 300, 400 };

    public string BuildingId => buildingId;
    public EmployeeDataSO EmployeeData => employeeData;
    public int MaxEmployees => Mathf.Max(1, maxEmployees);
    public int AutomaticHireCount => Mathf.Clamp(automaticHireCount, 0, MaxEmployees);

    public int GetAdditionalHireCost(int additionalHireIndex)
    {
        if (additionalHireCosts == null || additionalHireCosts.Length == 0) return 0;

        int index = Mathf.Clamp(additionalHireIndex, 0, additionalHireCosts.Length - 1);
        return Mathf.Max(0, additionalHireCosts[index]);
    }
    public bool IsValid => !string.IsNullOrWhiteSpace(buildingId) && employeeData != null;
}
