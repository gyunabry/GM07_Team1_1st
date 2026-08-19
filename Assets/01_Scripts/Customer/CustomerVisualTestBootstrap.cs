using System;
using System.Collections.Generic;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

// 손님 이동과 대기열을 눈으로 확인하기 위한 전용 테스트 씬 구성 요소
public sealed class CustomerVisualTestBootstrap : MonoBehaviour
{
    [SerializeField] private BuildingDataSO salesCounterData;

    private Camera testCamera;
    private CurrencySystem currencySystem;
    private TextMeshProUGUI currencyText;

    private void Awake()
    {
        CreateLight();
        CreateCamera();

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.localScale = new Vector3(2f, 1f, 2f);

        new GameObject("EmployeeManager").AddComponent<EmployeeManager>();
        new GameObject("TestCounterInventory").AddComponent<CounterInventory>();

        CreateCounter(new Vector3(-6f, 0f, 2.5f));
        CreateCounter(new Vector3(-2f, 0f, 2.5f));
        CreateCounter(new Vector3(2f, 0f, 2.5f));
        CreateCounter(new Vector3(6f, 0f, 2.5f));
        Transform entrance = CreateMarker("Entrance", new Vector3(0f, 0f, -9f), Vector3.forward);
        // 계산대 옆의 군중을 가로지르지 않도록 바깥쪽 통로를 따라 출구로 이동한다.
        Transform exitTurn = CreateMarker("ExitTurn", new Vector3(8.5f, 0f, 1.5f), Vector3.right);
        Transform exit = CreateMarker("Exit", new Vector3(8.5f, 0f, -9f), Vector3.forward);

        NavMeshSurface surface = floor.AddComponent<NavMeshSurface>();
        surface.BuildNavMesh();

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
