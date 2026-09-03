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

    [Header("Ï∞∏Ï°∞")]
    [SerializeField] private EmployeeManager employeeManager;

    private readonly List<CarrierCommandRowView> activeRows = new();
    private readonly List<ProductionBuilding> buildings = new();
    
    private CarrierCommandService commandService;
    private CarrierCommandType currentType = CarrierCommandType.Material;
    private InputManager inputManager;

    private bool isOpen;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        inputManager = FindFirstObjectByType<InputManager>();
        materialButton.onClick.AddListener(() => SelectType(CarrierCommandType.Material));
        productButton.onClick.AddListener(() => SelectType(CarrierCommandType.Product));
        SetVisible(false);
        isOpen = false;
    }

    private void OnEnable()
    {
        if (inputManager != null)
        {
            inputManager.OnCancelPressed += Hide;
        }
    }

    private void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.OnCancelPressed -= Hide;
        }
    }

    private void Start()
    {
        commandService = employeeManager != null ? employeeManager.GetComponent<CarrierCommandService>() : null;
        if (commandService == null)
        {
            Debug.LogWarning("CommandSystem???∞Í≤∞?òÏ? ?äÏïò?µÎãà??");
        }
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

        // IngredientUI?Ä ProductUI??Í∞ôÏ? ?ÑÏπò??Í≤πÏ≥ê ?àÏúºÎ©?ProductUIÍ∞Ä ???ûÏóê ?åÎçîÎßÅÎêú??
        // ?¥Ïö© Ïª®ÌÖå?¥ÎÑàÎß??ÑÌôò?òÎ©¥ ?¨Î£å ?âÏù¥ ProductUI ?§Ïóê Í∞Ä?§Ï?ÎØÄÎ°? Í∞???ùò ?ÅÏúÑ ?®ÎÑê???ÑÌôò?úÎã§.
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
