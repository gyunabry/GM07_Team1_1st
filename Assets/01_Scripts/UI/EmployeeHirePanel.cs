using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 선택한 직원 건물의 추가 고용을 처리하는 UI입니다.
/// </summary>
public sealed class EmployeeHirePanel : MonoBehaviour
{
    private const string HireButtonPath = "Production_Select/Buy_Button";

    private Button hireButton;
    private TMP_Text employeeCountText;
    private EmployeeManager employeeManager;
    private PlacedBuilding selectedBuilding;

    private void Awake()
    {
        Transform hireButtonTransform = transform.Find(HireButtonPath);
        if (hireButtonTransform != null)
        {
            hireButton = hireButtonTransform.GetComponent<Button>();
            if (hireButton == null)
            {
                hireButton = hireButtonTransform.gameObject.AddComponent<Button>();
                hireButton.targetGraphic = hireButtonTransform.GetComponent<Graphic>();
            }

            hireButton.onClick.AddListener(Hire);
        }

        employeeCountText = FindEmployeeCountText();
        employeeManager = FindFirstObjectByType<EmployeeManager>();
    }

    private void OnEnable()
    {
        if (employeeManager != null)
        {
            employeeManager.EmployeeHired += HandleEmployeeChanged;
            employeeManager.EmployeeRemoved += HandleEmployeeChanged;
            employeeManager.EmployeeHiringLimitChanged += Refresh;
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
            hiringLimit = employeeManager.GetHiringLimit(employees.Count > 0
                ? employees[0].Role
                : GetBuildingRole(selectedBuilding));
        }

        if (employeeCountText != null)
        {
            employeeCountText.text = $"{employeeCount}/{hiringLimit}";
        }

        if (hireButton != null)
        {
            hireButton.interactable = employeeManager != null && employeeManager.CanHireAdditional(selectedBuilding);
        }
    }

    private TMP_Text FindEmployeeCountText()
    {
        foreach (TMP_Text text in GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.name == "Emploee_Amount")
            {
                return text;
            }
        }

        return null;
    }

    private static EmployeeRole GetBuildingRole(PlacedBuilding building)
    {
        return building.GetComponent<HunterBuildingController>() != null
            ? EmployeeRole.Hunter
            : EmployeeRole.Carrier;
    }
}
