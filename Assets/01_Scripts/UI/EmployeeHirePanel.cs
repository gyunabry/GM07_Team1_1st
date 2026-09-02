using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ?�택??직원 건물??추�? 고용??처리?�는 UI?�니??
/// </summary>
public sealed class EmployeeHirePanel : MonoBehaviour
{
    [SerializeField] private Button hireButton;
    [SerializeField] private TMP_Text employeeCountText;
    [SerializeField] private TMP_Text hireCostText;
    [SerializeField] private TMP_Text attackDamageText;
    [SerializeField] private TMP_Text movementSpeedText;
    [SerializeField] private TMP_Text carryingCapacityText;

    private EmployeeManager employeeManager;
    private CurrencySystem currencySystem;
    private PlacedBuilding selectedBuilding;
    private EmployeeRole selectedRole;

    private void Awake()
    {
        employeeManager = FindFirstObjectByType<EmployeeManager>();
        currencySystem = CurrencySystem.Instance;
        hireButton?.onClick.AddListener(Hire);
    }

    private void OnEnable()
    {
        if (employeeManager != null)
        {
            employeeManager.EmployeeHired += HandleEmployeeChanged;
            employeeManager.EmployeeRemoved += HandleEmployeeChanged;
            employeeManager.EmployeeHiringLimitChanged += Refresh;
        }

        currencySystem ??= CurrencySystem.Instance;
        if (currencySystem != null)
        {
            currencySystem.CurrencyChanged += HandleCurrencyChanged;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (employeeManager != null)
        {
            employeeManager.EmployeeHired -= HandleEmployeeChanged;
            employeeManager.EmployeeRemoved -= HandleEmployeeChanged;
            employeeManager.EmployeeHiringLimitChanged -= Refresh;
        }

        if (currencySystem != null)
        {
            currencySystem.CurrencyChanged -= HandleCurrencyChanged;
        }
    }

    private void OnDestroy()
    {
        if (hireButton != null)
        {
            hireButton.onClick.RemoveListener(Hire);
        }
    }

    public void Bind(PlacedBuilding building)
    {
        selectedBuilding = building;
        selectedRole = GetBuildingRole(building);

        if (employeeManager == null)
        {
            employeeManager = FindFirstObjectByType<EmployeeManager>();
        }

        Refresh();
    }

    private void Hire()
    {
        if (employeeManager == null || selectedBuilding == null)
        {
            return;
        }

        employeeManager.TryHireAdditional(selectedBuilding, out _);
    }

    private void HandleCurrencyChanged(int money, int experience)
    {
        Refresh();
    }

    private void HandleEmployeeChanged(EmployeeRuntimeData employee)
    {
        Refresh();
    }

    private void Refresh()
    {
        int employeeCount = 0;
        int hiringLimit = 0;
        if (employeeManager != null && selectedBuilding != null &&
            employeeManager.TryGetEmployees(selectedBuilding, out var employees))
        {
            employeeCount = employees.Count;
            hiringLimit = employeeManager.GetHiringLimit(selectedRole);
        }

        if (employeeCountText != null)
        {
            employeeCountText.text = $"{employeeCount}/{hiringLimit}";
        }

        if (hireCostText != null)
        {
            hireCostText.text = employeeManager != null && selectedBuilding != null
                ? employeeManager.GetNextHireCost(selectedBuilding).ToString("N0")
                : "-";
        }

        if (hireButton != null)
        {
            hireButton.interactable = employeeManager != null && employeeManager.CanHireAdditional(selectedBuilding);
        }

        RefreshEmployeeStats();
    }

    private void RefreshEmployeeStats()
    {
        if (selectedBuilding == null)
        {
            SetEmployeeStats("-", "-", "-");
            return;
        }

        HunterBuildingController hunterBuilding = selectedBuilding.GetComponent<HunterBuildingController>();
        if (hunterBuilding != null && hunterBuilding.TryGetEmployeeStats(out float attackDamage, out float hunterMovementSpeed, out int hunterCarryingCapacity))
        {
            SetEmployeeStats(FormatNumber(attackDamage), FormatNumber(hunterMovementSpeed), hunterCarryingCapacity.ToString());
            return;
        }

        CarrierEmployeeBuildingController carrierBuilding = selectedBuilding.GetComponent<CarrierEmployeeBuildingController>();
        if (carrierBuilding != null && carrierBuilding.TryGetEmployeeStats(out float carrierMovementSpeed, out int carrierCarryingCapacity))
        {
            SetEmployeeStats("-", FormatNumber(carrierMovementSpeed), carrierCarryingCapacity.ToString());
            return;
        }

        SetEmployeeStats("-", "-", "-");
    }

    private void SetEmployeeStats(string attackDamage, string movementSpeed, string carryingCapacity)
    {
        if (attackDamageText != null)
        {
            attackDamageText.text = attackDamage;
        }

        if (movementSpeedText != null)
        {
            movementSpeedText.text = movementSpeed;
        }

        if (carryingCapacityText != null)
        {
            carryingCapacityText.text = carryingCapacity;
        }
    }

    private static string FormatNumber(float value)
    {
        return value.ToString("0.##");
    }

    private static EmployeeRole GetBuildingRole(PlacedBuilding building)
    {
        return building != null && building.GetComponent<HunterBuildingController>() != null
            ? EmployeeRole.Hunter
            : EmployeeRole.Carrier;
    }
}
