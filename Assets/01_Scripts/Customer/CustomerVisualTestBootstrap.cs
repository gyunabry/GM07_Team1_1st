using System;
using System.Collections.Generic;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

// 손님 이동과 대기열을 눈으로 확인하기 위한 전용 테스트 씬 구성 요소
public sealed class CustomerVisualTestBootstrap : MonoBehaviour
{
    private Camera testCamera;
    private Transform checkoutOperator;
    private CurrencySystem currencySystem;
    private TextMeshProUGUI currencyText;

    private void Awake()
    {
        CreateLight();
        CreateCamera();

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.localScale = new Vector3(2f, 1f, 2f);

        CreateCounter();
        Transform checkoutFront = CreateMarker("CheckoutFront", new Vector3(0f, 0f, 1.5f), Vector3.forward);
        Transform entrance = CreateMarker("Entrance", new Vector3(0f, 0f, -9f), Vector3.forward);
        Transform exitTurn = CreateMarker("ExitTurn", new Vector3(2.5f, 0f, 1.5f), Vector3.right);
        Transform exit = CreateMarker("Exit", new Vector3(2.5f, 0f, -9f), Vector3.forward);

        NavMeshSurface surface = floor.AddComponent<NavMeshSurface>();
        surface.BuildNavMesh();

        ShopCustomerQueue queue = new GameObject("ShopCustomerQueue").AddComponent<ShopCustomerQueue>();
        queue.Configure(checkoutFront, 1.25f, 6);

        ShopCheckout checkout = new GameObject("ShopCheckout").AddComponent<ShopCheckout>();
        checkout.transform.position = new Vector3(0f, 0f, 2f);
        checkout.ConfigureZone(new Vector3(3f, 2f, 2f));
        checkoutOperator = CreateCheckoutOperator(new Vector3(0f, 0f, 2f));

        CustomerController customerTemplate = CreateCustomerTemplate();
        new GameObject("PoolManager").AddComponent<PoolManager>();
        CustomerVisualTestServices testServices = new GameObject("TestServices").AddComponent<CustomerVisualTestServices>();
        currencySystem = new GameObject("CurrencySystem").AddComponent<CurrencySystem>();
        CustomerSpawnManager spawnManager = new GameObject("CustomerSpawnManager").AddComponent<CustomerSpawnManager>();
        CreateCurrencyDisplay();

        CustomerOrder order = new CustomerOrder
        {
            Items = new List<CustomerOrderItem>
            {
                new CustomerOrderItem { ItemId = "A", Amount = 3 },
                new CustomerOrderItem { ItemId = "B", Amount = 5 }
            },
            Reward = 10,
            ExperienceReward = 5
        };

        spawnManager.Configure(customerTemplate, queue, checkout, entrance, exitTurn, exit, order, 2.5f);
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

    private void Update()
    {
        if (testCamera == null || checkoutOperator == null || Mouse.current == null)
        {
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = testCamera.ScreenPointToRay(mousePosition);
        Plane floorPlane = new Plane(Vector3.up, Vector3.zero);
        if (floorPlane.Raycast(ray, out float distance))
        {
            checkoutOperator.position = ray.GetPoint(distance);
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

    private static void CreateCounter()
    {
        GameObject counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
        counter.name = "Counter";
        counter.transform.position = new Vector3(0f, 0.75f, 2.5f);
        counter.transform.localScale = new Vector3(4f, 1.5f, 1f);
    }

    private static Transform CreateCheckoutOperator(Vector3 position)
    {
        GameObject checkoutOperator = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        checkoutOperator.name = "CheckoutOperator";
        checkoutOperator.transform.position = position;
        checkoutOperator.AddComponent<CheckoutOperatorPresence>();
        return checkoutOperator.transform;
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
