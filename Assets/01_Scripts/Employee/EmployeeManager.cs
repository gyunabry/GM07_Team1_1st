using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 직원의 공통 고용 수명주기와 건물 소속만 관리합니다.
/// 건물 시스템은 구매/로드 완료 후 TryRegisterBuilding을, 판매 직전에는
/// TryUnregisterBuilding을 호출해야 합니다.
/// </summary>
public sealed class EmployeeManager : MonoBehaviour
{
    [SerializeField] private List<EmployeeBuildingProfile> buildingProfiles = new();

    private readonly Dictionary<int, RegisteredBuilding> registeredBuildings = new();
    private int nextEmployeeId = 1;

    public event Action<EmployeeRuntimeData> EmployeeHired;
    public event Action<EmployeeRuntimeData> EmployeeRemoved;
    public event Action<EmployeeRuntimeData> EmployeeWorkStateChanged;

    /// <summary>
    /// 직원 건물을 등록하고 프로필에 지정된 기본 인원을 자동 고용합니다.
    /// </summary>
    public bool TryRegisterBuilding(PlacedBuilding building)
    {
        if (building == null || building.Data == null)
        {
            return false;
        }

        int buildingKey = building.GetInstanceID();
        if (registeredBuildings.ContainsKey(buildingKey) || !TryGetProfile(building.Data.BuildingId, out EmployeeBuildingProfile profile))
        {
            return false;
        }

        RegisteredBuilding registeredBuilding = new(building, profile);
        registeredBuildings.Add(buildingKey, registeredBuilding);

        for (int i = 0; i < profile.AutomaticHireCount; i++)
        {
            Hire(registeredBuilding);
        }

        return true;
    }

    /// <summary>
    /// 건물에 소속된 모든 직원을 제거합니다. 건물 판매/파괴 전에 호출해야 합니다.
    /// </summary>
    public bool TryUnregisterBuilding(PlacedBuilding building)
    {
        if (building == null || !registeredBuildings.TryGetValue(building.GetInstanceID(), out RegisteredBuilding registeredBuilding))
        {
            return false;
        }

        for (int i = registeredBuilding.Employees.Count - 1; i >= 0; i--)
        {
            TryRemoveEmployee(registeredBuilding.Employees[i]);
        }

        registeredBuildings.Remove(building.GetInstanceID());
        return true;
    }

    /// <summary>
    /// 정원 내에서 직원 한 명을 추가 고용합니다. 비용 검증은 후속 경제 시스템 연동 시 추가합니다.
    /// </summary>
    public bool TryHireAdditional(PlacedBuilding building, out EmployeeRuntimeData employee)
    {
        employee = null;
        if (!TryGetRegisteredBuilding(building, out RegisteredBuilding registeredBuilding) || registeredBuilding.Employees.Count >= registeredBuilding.Profile.MaxEmployees)
        {
            return false;
        }

        employee = Hire(registeredBuilding);
        return true;
    }

    public bool TryRemoveEmployee(EmployeeRuntimeData employee)
    {
        if (employee == null || employee.AssignedBuilding == null || !TryGetRegisteredBuilding(employee.AssignedBuilding, out RegisteredBuilding registeredBuilding))
        {
            return false;
        }

        if (!registeredBuilding.Employees.Remove(employee))
        {
            return false;
        }

        employee.Release();
        EmployeeRemoved?.Invoke(employee);
        return true;
    }

    public bool TrySetWorkState(EmployeeRuntimeData employee, EmployeeWorkState workState)
    {
        if (employee == null || employee.AssignedBuilding == null || !TryGetRegisteredBuilding(employee.AssignedBuilding, out RegisteredBuilding registeredBuilding) || !registeredBuilding.Employees.Contains(employee))
        {
            return false;
        }

        if (employee.WorkState == workState)
        {
            return true;
        }

        employee.SetWorkState(workState);
        EmployeeWorkStateChanged?.Invoke(employee);
        return true;
    }

    public bool TryGetEmployees(PlacedBuilding building, out IReadOnlyList<EmployeeRuntimeData> employees)
    {
        if (TryGetRegisteredBuilding(building, out RegisteredBuilding registeredBuilding))
        {
            employees = registeredBuilding.Employees;
            return true;
        }

        employees = Array.Empty<EmployeeRuntimeData>();
        return false;
    }

    private EmployeeRuntimeData Hire(RegisteredBuilding registeredBuilding)
    {
        EmployeeRuntimeData employee = new(nextEmployeeId++, registeredBuilding.Profile.EmployeeData, registeredBuilding.Building);
        registeredBuilding.Employees.Add(employee);
        EmployeeHired?.Invoke(employee);
        return employee;
    }

    private bool TryGetRegisteredBuilding(PlacedBuilding building, out RegisteredBuilding registeredBuilding)
    {
        registeredBuilding = null;
        return building != null && registeredBuildings.TryGetValue(building.GetInstanceID(), out registeredBuilding);
    }

    private bool TryGetProfile(string buildingId, out EmployeeBuildingProfile profile)
    {
        profile = null;
        if (string.IsNullOrWhiteSpace(buildingId))
        {
            return false;
        }

        for (int i = 0; i < buildingProfiles.Count; i++)
        {
            EmployeeBuildingProfile candidate = buildingProfiles[i];
            if (candidate != null && candidate.IsValid && string.Equals(candidate.BuildingId, buildingId, StringComparison.Ordinal))
            {
                profile = candidate;
                return true;
            }
        }

        return false;
    }

    private sealed class RegisteredBuilding
    {
        public PlacedBuilding Building { get; }
        public EmployeeBuildingProfile Profile { get; }
        public List<EmployeeRuntimeData> Employees { get; } = new();

        public RegisteredBuilding(PlacedBuilding building, EmployeeBuildingProfile profile)
        {
            Building = building;
            Profile = profile;
        }
    }
}
