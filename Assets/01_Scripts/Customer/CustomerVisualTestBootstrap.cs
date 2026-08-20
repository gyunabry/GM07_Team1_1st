using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// 손님 이동과 대기열을 눈으로 확인하기 위한 전용 테스트 씬 구성 요소
public sealed class CustomerVisualTestBootstrap : MonoBehaviour
{
    [SerializeField] private BuildingDataSO salesCounterData;
    [SerializeField] private BuildingDataSO productionBuildingData;
    [SerializeField] private CarrierWorker carrierEmployeePrefab;

    private Camera testCamera;
    private CurrencySystem currencySystem;
    private TextMeshProUGUI currencyText;
    private CarrierWorker testCarrier;
    private ProductionBuilding testProductionBuilding;
    private ItemInventory testTransmitterInventory;
    private TextMeshProUGUI carrierCommandStatusText;
    private Button materialAddButton;
    private Button materialRemoveButton;
    private Button productAddButton;
    private Button productRemoveButton;

    private void Awake()
    {
        CreateLight();
        CreateCamera();

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.localScale = new Vector3(2f, 1f, 2f);

        // 테스트 건물은 NavMesh를 막지 않게 바닥만 있는 상태에서 먼저 굽는다.
        NavMeshSurface surface = floor.AddComponent<NavMeshSurface>();
        surface.BuildNavMesh();

        new GameObject("EmployeeManager").AddComponent<EmployeeManager>();
        new GameObject("TestCounterInventory").AddComponent<CounterInventory>();

        ProductionBuilding productionBuilding = CreateProductionBuilding(new Vector3(-1.5f, 0f, -3.5f));
        // 생산 건물 반대편에 임시 운반 직원 건물과 전송기를 둔다.
        Transform carrierHome = CreateTemporaryCarrierBuilding(new Vector3(7f, 0f, -3.5f));
        Transform transmitterWorkPoint = CreateTemporaryTransmitter(new Vector3(4.5f, 0f, -3.5f));
        StartCoroutine(MoveTestTransmitterAfterDelay(transmitterWorkPoint));

        CreateCounter(new Vector3(-6f, 0f, 2.5f));
        CreateCounter(new Vector3(-2f, 0f, 2.5f));
        CreateCounter(new Vector3(2f, 0f, 2.5f));
        CreateCounter(new Vector3(6f, 0f, 2.5f));
        Transform entrance = CreateMarker("Entrance", new Vector3(0f, 0f, -9f), Vector3.forward);
        // 계산대 옆의 군중을 가로지르지 않도록 바깥쪽 통로를 따라 출구로 이동한다.
        Transform exitTurn = CreateMarker("ExitTurn", new Vector3(8.5f, 0f, 1.5f), Vector3.right);
        Transform exit = CreateMarker("Exit", new Vector3(8.5f, 0f, -9f), Vector3.forward);

        CustomerController customerTemplate = CreateCustomerTemplate();
        customerTemplate.ConfigureDefaultOrder(new CustomerOrder
        {
            Items = new List<CustomerOrderItem>
            {
                new CustomerOrderItem { ItemId = ScriptableObject.CreateInstance<ItemDataSO>(), Amount = 3 },
                new CustomerOrderItem { ItemId = ScriptableObject.CreateInstance<ItemDataSO>(), Amount = 5 }
            },
            Reward = 10,
            ExperienceReward = 5
        });
        new GameObject("PoolManager").AddComponent<PoolManager>();
        CustomerVisualTestServices testServices = new GameObject("TestServices").AddComponent<CustomerVisualTestServices>();
        currencySystem = new GameObject("CurrencySystem").AddComponent<CurrencySystem>();
        CustomerSpawnManager spawnManager = new GameObject("CustomerSpawnManager").AddComponent<CustomerSpawnManager>();
        CreateCurrencyDisplay();

        spawnManager.Configure(customerTemplate, entrance, exitTurn, exit, 2.5f);
        spawnManager.BindServices(testServices, currencySystem);
    }

    // CustomerVisualTest에서만 사용하는 임시 재화 표시 UI다.
    private void CreateCurrencyDisplay()
    {
        GameObject canvasObject = new GameObject("CurrencyDebugCanvas", typeof(Canvas));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        GameObject textObject = new GameObject("CurrencyText", typeof(RectTransform));
        textObject.transform.SetParent(canvasObject.transform, false);

        currencyText = textObject.AddComponent<TextMeshProUGUI>();
        currencyText.font = TMP_Settings.defaultFontAsset;
        currencyText.fontSize = 32f;
        currencyText.color = Color.white;
        currencyText.alignment = TextAlignmentOptions.TopLeft;

        RectTransform rectTransform = currencyText.rectTransform;
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = new Vector2(24f, -24f);
        rectTransform.sizeDelta = new Vector2(360f, 100f);

        currencySystem.CurrencyChanged += RefreshCurrencyText;
        RefreshCurrencyText(currencySystem.Money, currencySystem.Experience);
    }

    private void RefreshCurrencyText(int money, int experience)
    {
        if (currencyText != null)
        {
            currencyText.text = $"Money: {money}\nExperience: {experience}";
        }
    }

    private void OnDestroy()
    {
        if (currencySystem != null)
        {
            currencySystem.CurrencyChanged -= RefreshCurrencyText;
        }
    }

    private static CustomerController CreateCustomerTemplate()
    {
        GameObject customer = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        customer.name = "CustomerTemplate";
        customer.transform.position = new Vector3(0f, -10f, 0f);

        NavMeshAgent agent = customer.AddComponent<NavMeshAgent>();
        agent.speed = 1.5f;
        agent.angularSpeed = 360f;
        agent.acceleration = 8f;
        agent.radius = 0.3f;
        agent.stoppingDistance = 0.1f;

        customer.AddComponent<CustomerStateMachine>();
        CustomerController controller = customer.AddComponent<CustomerController>();
        customer.SetActive(false);
        return controller;
    }

    private void CreateCounter(Vector3 position)
    {
        if (salesCounterData == null || salesCounterData.BuildingPrefab == null)
        {
            Debug.LogError("CustomerVisualTestBootstrap requires the sales counter building data.", this);
            return;
        }

        GameObject counter = Instantiate(salesCounterData.BuildingPrefab, position, Quaternion.identity);
        counter.name = salesCounterData.BuildingName;

        PlacedBuilding placedBuilding = counter.GetComponent<PlacedBuilding>();
        if (placedBuilding == null)
        {
            Debug.LogError("Sales counter prefab requires PlacedBuilding.", counter);
            Destroy(counter);
            return;
        }

        Vector3Int cell = Vector3Int.RoundToInt(position);
        placedBuilding.Initialize(salesCounterData, cell, 0, new[] { cell });
        placedBuilding.BeginConstruction();
    }

    private void Update()
    {
        RefreshCarrierCommandUi();
    }

    private ProductionBuilding CreateProductionBuilding(Vector3 position)
    {
        if (productionBuildingData == null || productionBuildingData.BuildingPrefab == null)
        {
            Debug.LogError("CustomerVisualTestBootstrap requires the production building data.", this);
            return null;
        }

        GameObject productionObject = Instantiate(productionBuildingData.BuildingPrefab, position, Quaternion.identity);
        productionObject.name = productionBuildingData.BuildingName;

        PlacedBuilding placedBuilding = productionObject.GetComponent<PlacedBuilding>();
        ProductionBuilding productionBuilding = productionObject.GetComponent<ProductionBuilding>();
        if (placedBuilding == null || productionBuilding == null)
        {
            Debug.LogError("Production building prefab requires PlacedBuilding and ProductionBuilding.", productionObject);
            Destroy(productionObject);
            return null;
        }

        Vector3Int cell = Vector3Int.RoundToInt(position);
        placedBuilding.Initialize(productionBuildingData, cell, 0, new[] { cell });
        placedBuilding.BeginConstruction();
        return productionBuilding;
    }

    private static Transform CreateTemporaryCarrierBuilding(Vector3 position)
    {
        GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
        building.name = "TestCarrierEmployeeBuilding";
        building.transform.position = position + new Vector3(0f, 0.75f, 0f);
        building.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        building.GetComponent<Renderer>().material.color = new Color(0.35f, 0.8f, 0.45f);

        return CreateMarker("CarrierHome", position + new Vector3(1.2f, 0f, 0f), Vector3.right);
    }

    private static Transform CreateTemporaryTransmitter(Vector3 position)
    {
        GameObject transmitter = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        transmitter.name = "TestCarrierTransmitter";
        transmitter.transform.position = position + new Vector3(0f, 0.5f, 0f);
        transmitter.transform.localScale = new Vector3(1f, 0.5f, 1f);
        transmitter.GetComponent<Renderer>().material.color = new Color(0.25f, 0.55f, 0.95f);

        Transform workPoint = CreateMarker("CarrierTransmitterWorkPoint", position + new Vector3(0.8f, 0f, 0f), Vector3.right);
        transmitter.transform.SetParent(workPoint, true);
        return workPoint;
    }

    private IEnumerator MoveTestTransmitterAfterDelay(Transform transmitterWorkPoint)
    {
        yield return new WaitForSeconds(10f);

        if (transmitterWorkPoint != null)
        {
            transmitterWorkPoint.position += new Vector3(0f, 0f, 4f);
        }
    }

    private IEnumerator SpawnCarrierForTest(PoolManager poolManager, ProductionBuilding productionBuilding, Transform homePoint, ItemInventory transmitterInventory, Transform transmitterWorkPoint)
    {
        yield return null;

        testCarrier = poolManager.GetPool(carrierEmployeePrefab);
        EmployeeDataSO employeeData = EmployeeDataSO.CreateRuntime("test-carrier", "테스트 운반 직원", EmployeeRole.Carrier);
        EmployeeRuntimeData employee = new(1, employeeData, null);
        testCarrier.transform.position = homePoint.position;
        testCarrier.Initialize(null, employee, homePoint);
        testCarrier.ConfigureLogistics(transmitterInventory, transmitterWorkPoint);
    }

    private void CreateCarrierCommandUi(ProductionBuilding productionBuilding)
    {
        EnsureInputSystemEventSystem();

        GameObject canvasObject = new GameObject("CarrierCommandTestCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 110;
        CanvasScaler canvasScaler = canvasObject.GetComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.matchWidthOrHeight = 0.5f;

        RectTransform panel = CreateUiPanel(canvasObject.transform, new Vector2(-30f, -30f), new Vector2(560f, 400f));
        float y = -24f;
        CreateUiText(panel, "Carrier Commands (Test)", new Vector2(20f, y), 30f, Color.white);
        y -= 52f;
        carrierCommandStatusText = CreateUiText(panel, "Loading carrier...", new Vector2(20f, y), 21f, new Color(0.8f, 0.9f, 1f));
        y -= 58f;
        string initialRecipeId = productionBuilding.SelectedRecipe != null ? productionBuilding.SelectedRecipe.RecipeId : "None";
        CreateUiText(panel, $"Auto Recipe: {initialRecipeId}", new Vector2(20f, y), 23f, Color.white);
        y -= 42f;
        CreateUiText(panel, "Material: Transmitter -> Production", new Vector2(20f, y), 19f, Color.white);
        materialAddButton = CreateUiButton(panel, "+", new Vector2(408f, y + 4f), new Vector2(56f, 36f), () => TryAssignCarrierCommand(CarrierCommandType.Material));
        materialRemoveButton = CreateUiButton(panel, "-", new Vector2(474f, y + 4f), new Vector2(56f, 36f), () => TryClearCarrierCommand(CarrierCommandType.Material));
        y -= 50f;
        CreateUiText(panel, "Product: Production -> Counter", new Vector2(20f, y), 19f, Color.white);
        productAddButton = CreateUiButton(panel, "+", new Vector2(408f, y + 4f), new Vector2(56f, 36f), () => TryAssignCarrierCommand(CarrierCommandType.Product));
        productRemoveButton = CreateUiButton(panel, "-", new Vector2(474f, y + 4f), new Vector2(56f, 36f), () => TryClearCarrierCommand(CarrierCommandType.Product));
    }

    private static void EnsureInputSystemEventSystem()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            eventSystem = new GameObject("TestEventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
        }

        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
        {
            inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        BaseInputModule[] modules = eventSystem.GetComponents<BaseInputModule>();
        for (int i = 0; i < modules.Length; i++)
        {
            BaseInputModule module = modules[i];
            if (module != inputModule)
            {
                module.enabled = false;
                UnityEngine.Object.Destroy(module);
            }
        }

        inputModule.AssignDefaultActions();
    }

    private void RefreshCarrierCommandUi()
    {
        if (carrierCommandStatusText == null)
        {
            return;
        }

        bool isAvailable = testCarrier != null && testCarrier.IsAvailableForCommand;
        bool hasMaterialCommand = HasCarrierCommand(CarrierCommandType.Material);
        bool hasProductCommand = HasCarrierCommand(CarrierCommandType.Product);
        string selectedRecipeName = testProductionBuilding != null && testProductionBuilding.SelectedRecipe != null
            ? testProductionBuilding.SelectedRecipe.RecipeId
            : "None";

        carrierCommandStatusText.text = $"Selected: {selectedRecipeName}\nAvailable: {(isAvailable ? 1 : 0)} / Material: {(hasMaterialCommand ? 1 : 0)} / Product: {(hasProductCommand ? 1 : 0)}";
        if (materialAddButton != null) materialAddButton.interactable = isAvailable;
        if (productAddButton != null) productAddButton.interactable = isAvailable;
        if (materialRemoveButton != null) materialRemoveButton.interactable = hasMaterialCommand;
        if (productRemoveButton != null) productRemoveButton.interactable = hasProductCommand;
    }

    private void TryAssignCarrierCommand(CarrierCommandType type)
    {
        if (testCarrier != null)
        {
            testCarrier.TryAssignCommand(type, testProductionBuilding);
        }
    }

    private void TryClearCarrierCommand(CarrierCommandType type)
    {
        if (HasCarrierCommand(type))
        {
            testCarrier.ClearCommandFromUi();
        }
    }

    private bool HasCarrierCommand(CarrierCommandType type)
    {
        return testCarrier != null && testCarrier.HasCommand && testCarrier.CurrentCommand.Type == type && testCarrier.CurrentCommand.TargetBuilding == testProductionBuilding;
    }

    private static RectTransform CreateUiPanel(Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject panelObject = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(parent, false);
        Image image = panelObject.GetComponent<Image>();
        image.color = new Color(0.08f, 0.1f, 0.14f, 0.92f);

        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return rect;
    }

    private TextMeshProUGUI CreateUiText(Transform parent, string text, Vector2 anchoredPosition, float fontSize, Color color)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = text;
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = TextAlignmentOptions.TopLeft;

        RectTransform rect = label.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(520f, 50f);
        return label;
    }

    private Button CreateUiButton(Transform parent, string text, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject($"Button_{text}", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.28f, 0.42f, 0.58f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TextMeshProUGUI label = CreateUiText(buttonObject.transform, text, Vector2.zero, 21f, Color.white);
        label.alignment = TextAlignmentOptions.Center;
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        label.rectTransform.anchoredPosition = Vector2.zero;
        label.rectTransform.sizeDelta = Vector2.zero;
        return button;
    }


    private static Transform CreateMarker(string markerName, Vector3 position, Vector3 forward)
    {
        GameObject marker = new GameObject(markerName);
        marker.transform.SetPositionAndRotation(position, Quaternion.LookRotation(forward));
        return marker.transform;
    }

    private void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        testCamera = cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
        testCamera.transform.position = new Vector3(0f, 13f, -16f);
        testCamera.transform.LookAt(new Vector3(0f, 0f, 0f));
    }

    private static void CreateLight()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }
}

// 테스트 씬에서만 사용하는 임시 결제 서비스
public sealed class CustomerVisualTestServices : MonoBehaviour, ICustomerInventory
{
    public event Action InventoryChanged;

    public bool TryConsumeAll(IReadOnlyList<CustomerOrderItem> items)
    {
        bool canConsume = items != null && items.Count > 0;
        if (canConsume)
        {
            InventoryChanged?.Invoke();
        }

        return canConsume;
    }
}
