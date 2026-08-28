using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ì§ì›??ê³µí†µ ê³ ìš© ?˜ëª…ì£¼ê¸°?€ ê±´ë¬¼ ?Œì†ë§?ê´€ë¦¬í•©?ˆë‹¤.
/// ê±´ë¬¼ ?œìŠ¤?œì? êµ¬ë§¤/ë¡œë“œ ?„ë£Œ ??TryRegisterBuilding?? ?ë§¤ ì§ì „?ëŠ”
/// TryUnregisterBuilding???¸ì¶œ?´ì•¼ ?©ë‹ˆ??
/// </summary>
public sealed class EmployeeManager : MonoBehaviour
{
    private static readonly int[] HunterHiringLimitsByLevel =
    {
        0, 0, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 5, 5, 5
    };

    private static readonly int[] CarrierHiringLimitsByLevel =
    {
        0, 0, 0, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20
    };

    [SerializeField] private List<EmployeeBuildingProfile> buildingProfiles = new();

    private readonly Dictionary<int, RegisteredBuilding> registeredBuildings = new();
    private int nextEmployeeId = 1;
    private EmployeeDataSO runtimeSalesEmployeeData;
    private float hunterAttackDamageIncreasePercent;
    private float hunterAttackIntervalReductionPercent;
    private float hunterAttackRangeIncreasePercent;
    private float salesPaymentTimeReductionPercent;
    private float carrierItemTransferTimeReductionPercent;
    private float allEmployeeProcessingSpeedIncreasePercent;
    private float allEmployeeMovementSpeedIncreasePercent;
    private CurrencySystem currencySystem;

    public event Action<EmployeeRuntimeData> EmployeeHired;
    public event Action<EmployeeRuntimeData> EmployeeRemoved;
    public event Action<EmployeeRuntimeData> EmployeeWorkStateChanged;
    public event Action<float, float, float> HunterSkillModifiersChanged;
    public event Action<float> SalesPaymentTimeReductionChanged;
    public event Action<float> CarrierTransferTimeReductionChanged;
    public event Action<float> AllEmployeeProcessingSpeedIncreaseChanged;
    public event Action<float> AllEmployeeMovementSpeedIncreaseChanged;
    public event Action EmployeeHiringLimitChanged;

    public float HunterAttackDamageIncreasePercent => hunterAttackDamageIncreasePercent;
    public float HunterAttackIntervalReductionPercent => hunterAttackIntervalReductionPercent;
    public float HunterAttackRangeIncreasePercent => hunterAttackRangeIncreasePercent;
    public float SalesPaymentTimeReductionPercent => salesPaymentTimeReductionPercent;
    public float CarrierItemTransferTimeReductionPercent => carrierItemTransferTimeReductionPercent;
    public float AllEmployeeProcessingSpeedIncreasePercent => allEmployeeProcessingSpeedIncreasePercent;
    public float AllEmployeeMovementSpeedIncreasePercent => allEmployeeMovementSpeedIncreasePercent;
    public int HunterEmployeeCount => GetHiredEmployeeCount(EmployeeRole.Hunter);
    public int CarrierEmployeeCount => GetHiredEmployeeCount(EmployeeRole.Carrier);

    public void SetHunterAttackDamageIncreasePercent(float percent)
    {
        hunterAttackDamageIncreasePercent = Mathf.Max(0f, percent);
        NotifyHunterSkillModifiersChanged();
    }

    // Reduces attack interval to increase attack speed.
    public void SetHunterAttackIntervalReductionPercent(float percent)
    {
        hunterAttackIntervalReductionPercent = Mathf.Clamp(percent, 0f, 100f);
        NotifyHunterSkillModifiersChanged();
    }

    public void SetHunterAttackRangeIncreasePercent(float percent)
    {
        hunterAttackRangeIncreasePercent = Mathf.Max(0f, percent);
        NotifyHunterSkillModifiersChanged();
    }

    public void SetSalesPaymentTimeReductionPercent(float percent)
    {
        salesPaymentTimeReductionPercent = Mathf.Clamp(percent, 0f, 100f);
        SalesPaymentTimeReductionChanged?.Invoke(salesPaymentTimeReductionPercent);
    }

    public void SetCarrierItemTransferTimeReductionPercent(float percent)
    {
        carrierItemTransferTimeReductionPercent = Mathf.Clamp(percent, 0f, 100f);
        CarrierTransferTimeReductionChanged?.Invoke(carrierItemTransferTimeReductionPercent);
    }

    public void SetAllEmployeeProcessingSpeedIncreasePercent(float percent)
    {
        allEmployeeProcessingSpeedIncreasePercent = Mathf.Clamp(percent, 0f, 100f);
        AllEmployeeProcessingSpeedIncreaseChanged?.Invoke(allEmployeeProcessingSpeedIncreasePercent);
    }

    public void SetAllEmployeeMovementSpeedIncreasePercent(float percent)
    {
        allEmployeeMovementSpeedIncreasePercent = Mathf.Max(0f, percent);
        AllEmployeeMovementSpeedIncreaseChanged?.Invoke(allEmployeeMovementSpeedIncreasePercent);
    }

    // HUD???´ë°˜ ì§ì› ê±´ë¬¼ ?¤ì¹˜ ?„ì—???„ì—­ ëª…ë ¹ ?œë¹„?¤ë? ì¡°íšŒ?????ˆì–´???©ë‹ˆ??
    private void Awake()
    {
        if (GetComponent<CarrierCommandService>() == null)
        {
            gameObject.AddComponent<CarrierCommandService>();
        }
    }

    private void Start()
    {
        currencySystem = CurrencySystem.Instance;
        if (currencySystem != null)
        {
            currencySystem.LevelUp += HandleLevelUp;
        }
    }

    private void OnDestroy()
    {
        if (currencySystem != null)
        {
            currencySystem.LevelUp -= HandleLevelUp;
        }
    }

    /// <summary>
    /// ì§ì› ê±´ë¬¼???±ë¡?˜ê³  ?„ë¡œ?„ì— ì§€?•ëœ ê¸°ë³¸ ?¸ì›???ë™ ê³ ìš©?©ë‹ˆ??
    /// </summary>
    public bool TryRegisterBuilding(PlacedBuilding building)
    {
        if (building == null || building.Data == null)
        {
            return false;
        }

        if (!TryGetProfile(building.Data.BuildingId, out EmployeeBuildingProfile profile))
        {
            return false;
        }

        return TryRegisterBuilding(building, profile.EmployeeData, profile.MaxEmployees, profile.AutomaticHireCount);
    }

    /// <summary>
    /// ?ë§¤?€??ê³ ì •???ë§¤ ì§ì› ??ëª…ì„ ?±ë¡?©ë‹ˆ??
    /// ?ë§¤ ì§ì›?€ ë³„ë„ Inspector ?„ë¡œ???†ì´ ?°í????°ì´?°ë¡œ ê´€ë¦¬í•©?ˆë‹¤.
    /// </summary>
    public bool TryRegisterSalesBuilding(PlacedBuilding building)
    {
        return TryRegisterBuilding(building, GetOrCreateRuntimeSalesEmployeeData(), 1, 1);
    }

    /// <summary>
    /// ê±´ë¬¼???Œì†??ëª¨ë“  ì§ì›???œê±°?©ë‹ˆ?? ê±´ë¬¼ ?ë§¤/?Œê´´ ?„ì— ?¸ì¶œ?´ì•¼ ?©ë‹ˆ??
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
    /// ?•ì› ?´ì—??ì§ì› ??ëª…ì„ ì¶”ê? ê³ ìš©?©ë‹ˆ?? ë¹„ìš© ê²€ì¦ì? ?„ì† ê²½ì œ ?œìŠ¤???°ë™ ??ì¶”ê??©ë‹ˆ??
    /// </summary>
    public bool TryHireAdditional(PlacedBuilding building, out EmployeeRuntimeData employee)
    {
        employee = null;
        if (!TryGetRegisteredBuilding(building, out RegisteredBuilding registeredBuilding) || !CanHire(registeredBuilding))
        {
            return false;
        }

        employee = Hire(registeredBuilding);
        return true;
    }

    public bool CanHireAdditional(PlacedBuilding building)
    {
        return TryGetRegisteredBuilding(building, out RegisteredBuilding registeredBuilding) && CanHire(registeredBuilding);
    }

    public int GetHiringLimit(EmployeeRole role)
    {
        CurrencySystem currentCurrencySystem = currencySystem != null ? currencySystem : CurrencySystem.Instance;
        if (currentCurrencySystem == null)
        {
            return int.MaxValue;
        }

        int level = currentCurrencySystem.Level;
        return role switch
        {
            EmployeeRole.Hunter => GetLimitForLevel(HunterHiringLimitsByLevel, level),
            EmployeeRole.Carrier => GetLimitForLevel(CarrierHiringLimitsByLevel, level),
            _ => int.MaxValue
        };
    }

    public int GetHiredEmployeeCount(EmployeeRole role)
    {
        int count = 0;
        foreach (RegisteredBuilding registeredBuilding in registeredBuildings.Values)
        {
            if (registeredBuilding.EmployeeData.Role == role)
            {
                count += registeredBuilding.Employees.Count;
            }
        }

        return count;
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
        EmployeeRuntimeData employee = new(nextEmployeeId++, registeredBuilding.EmployeeData, registeredBuilding.Building);
        registeredBuilding.Employees.Add(employee);
        EmployeeHired?.Invoke(employee);
        return employee;
    }

    private bool CanHire(RegisteredBuilding registeredBuilding)
    {
        if (registeredBuilding == null || registeredBuilding.Employees.Count >= registeredBuilding.MaxEmployees)
        {
            return false;
        }

        // Á÷±ºº° ·¹º§ ÇÑµµ´Â ÀüÃ¼ Á÷¿ø ¼ö°¡ ¾Æ´Ï¶ó °Ç¹° ÇÑ Ã¤´ç °í¿ëÇÒ ¼ö ÀÖ´Â ÀÎ¿øÀÌ´Ù.
        return registeredBuilding.Employees.Count < GetHiringLimit(registeredBuilding.EmployeeData.Role);
    }

    private static int GetLimitForLevel(int[] limits, int level)
    {
        int index = Mathf.Clamp(level, 1, limits.Length - 1);
        return limits[index];
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

    private bool TryRegisterBuilding(PlacedBuilding building, EmployeeDataSO employeeData, int maxEmployees, int automaticHireCount)
    {
        if (building == null || employeeData == null || registeredBuildings.ContainsKey(building.GetInstanceID()))
        {
            return false;
        }

        RegisteredBuilding registeredBuilding = new(building, employeeData, maxEmployees);
        registeredBuildings.Add(building.GetInstanceID(), registeredBuilding);

        int hireCount = Mathf.Clamp(automaticHireCount, 0, registeredBuilding.MaxEmployees);
        for (int i = 0; i < hireCount; i++)
        {
            if (!CanHire(registeredBuilding))
            {
                break;
            }

            Hire(registeredBuilding);
        }

        return true;
    }

    private EmployeeDataSO GetOrCreateRuntimeSalesEmployeeData()
    {
        if (runtimeSalesEmployeeData == null)
        {
            runtimeSalesEmployeeData = EmployeeDataSO.CreateRuntime("runtime-sales-employee", "?ë§¤ ì§ì›", EmployeeRole.Sales);
        }

        return runtimeSalesEmployeeData;
    }

    private void NotifyHunterSkillModifiersChanged()
    {
        HunterSkillModifiersChanged?.Invoke(
            hunterAttackDamageIncreasePercent,
            hunterAttackIntervalReductionPercent,
            hunterAttackRangeIncreasePercent);
    }

    private void HandleLevelUp()
    {
        EmployeeHiringLimitChanged?.Invoke();
    }

    private sealed class RegisteredBuilding
    {
        public PlacedBuilding Building { get; }
        public EmployeeDataSO EmployeeData { get; }
        public int MaxEmployees { get; }
        public List<EmployeeRuntimeData> Employees { get; } = new();

        public RegisteredBuilding(PlacedBuilding building, EmployeeDataSO employeeData, int maxEmployees)
        {
            Building = building;
            EmployeeData = employeeData;
            MaxEmployees = Mathf.Max(1, maxEmployees);
        }
    }
}
