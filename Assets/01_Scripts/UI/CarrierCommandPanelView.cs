using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CarrierCommandPanelView : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button materialButton;
    [SerializeField] private Button productButton;

    [Header("Material List")]
    [SerializeField] private Transform materialContent;
    [SerializeField] private CarrierCommandRowView materialRowTemplate;

    [Header("Product List")]
    [SerializeField] private Transform productContent;
    [SerializeField] private CarrierCommandRowView productRowTemplate;

    private readonly List<CarrierCommandRowView> activeRows = new();
    private readonly List<ProductionBuilding> buildings = new();
    private EmployeeManager employeeManager;
    private CarrierCommandService commandService;
    private CarrierCommandType currentType = CarrierCommandType.Material;

    private bool isOpen;

    private void Awake()
    {
        employeeManager = UnityEngine.Object.FindFirstObjectByType<EmployeeManager>();
        commandService = employeeManager != null ? employeeManager.GetComponent<CarrierCommandService>() : null;

        materialButton.onClick.AddListener(() => SelectType(CarrierCommandType.Material));
        productButton.onClick.AddListener(() => SelectType(CarrierCommandType.Product));
        SetVisible(false);
        isOpen = false;
    }

    private void OnDestroy()
    {
        if (employeeManager == null) return;
        employeeManager.EmployeeHired -= HandleEmployeeChanged;
        employeeManager.EmployeeRemoved -= HandleEmployeeChanged;
    }

    public void Toggle()
    {
        if (isOpen)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    public void Show()
    {
        isOpen = true;
        SetVisible(true);
    }

    public void Hide()
    {
        isOpen = false;
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;

        if (!visible) return;

        if (employeeManager != null)
        {
            employeeManager.EmployeeHired -= HandleEmployeeChanged;
            employeeManager.EmployeeRemoved -= HandleEmployeeChanged;
            employeeManager.EmployeeHired += HandleEmployeeChanged;
            employeeManager.EmployeeRemoved += HandleEmployeeChanged;
        }

        SelectType(currentType);
    }

    private void SelectType(CarrierCommandType type)
    {
        currentType = type;

        // IngredientUI와 ProductUI는 같은 위치에 겹쳐 있으며 ProductUI가 더 앞에 렌더링된다.
        // 내용 컨테이너만 전환하면 재료 행이 ProductUI 뒤에 가려지므로, 각 탭의 상위 패널을 전환한다.
        materialContent.parent.gameObject.SetActive(type == CarrierCommandType.Material);
        productContent.parent.gameObject.SetActive(type == CarrierCommandType.Product);

        materialButton.interactable = type != CarrierCommandType.Material;
        productButton.interactable = type != CarrierCommandType.Product;
        RebuildRows();
    }

    private void RebuildRows()
    {
        foreach (CarrierCommandRowView row in activeRows)
        {
            if (row != null) Destroy(row.gameObject);
        }

        activeRows.Clear();
        buildings.Clear();
        buildings.AddRange(UnityEngine.Object.FindObjectsByType<ProductionBuilding>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .Where(IsCommandTarget)
            .OrderBy(building => building.name, StringComparer.Ordinal));

        Transform content = currentType == CarrierCommandType.Material ? materialContent : productContent;
        CarrierCommandRowView template = currentType == CarrierCommandType.Material ? materialRowTemplate : productRowTemplate;
        for (int index = 0; index < buildings.Count; index++)
        {
            CarrierCommandRowView row = Instantiate(template, content);
            row.gameObject.SetActive(true);
            row.Bind(buildings[index], currentType, commandService, GetTotalCarrierCount, RefreshRows);
            activeRows.Add(row);
        }
    }

    private void RefreshRows()
    {
        foreach (CarrierCommandRowView row in activeRows)
        {
            if (row != null) row.Refresh();
        }
    }

    private int GetTotalCarrierCount()
    {
        if (employeeManager == null) return 0;
        int count = 0;
        foreach (CarrierEmployeeBuildingController controller in UnityEngine.Object.FindObjectsByType<CarrierEmployeeBuildingController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            PlacedBuilding placedBuilding = controller.GetComponent<PlacedBuilding>();
            if (placedBuilding == null || !employeeManager.TryGetEmployees(placedBuilding, out IReadOnlyList<EmployeeRuntimeData> employees)) continue;
            count += employees.Count(employee => employee != null && employee.Role == EmployeeRole.Carrier);
        }

        return count;
    }

    private void HandleEmployeeChanged(EmployeeRuntimeData _)
    {
        RefreshRows();
    }

    private static bool IsCommandTarget(ProductionBuilding building)
    {
        return building != null && building.CanOperate && building.SelectedRecipe != null;
    }
}
