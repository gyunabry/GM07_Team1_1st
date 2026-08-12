using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public sealed class CustomerCheckoutStation
{
    [SerializeField] private ShopCustomerQueue queue;
    [SerializeField] private ShopCheckout checkout;

    public CustomerCheckoutStation(ShopCustomerQueue queue, ShopCheckout checkout)
    {
        this.queue = queue;
        this.checkout = checkout;
    }

    public ShopCustomerQueue Queue => queue;
    public ShopCheckout Checkout => checkout;
    public bool IsValid => queue != null && checkout != null;
}

// 설정된 간격으로 PoolManager에서 손님을 대여해 하나의 상점 대기열에 참가
public sealed class CustomerSpawnManager : MonoBehaviour
{
    [SerializeField] private CustomerController customerPrefab;
    [SerializeField] private CustomerCheckoutStation[] checkoutStations;
    [SerializeField] private ShopCustomerQueue shopQueue;
    [SerializeField] private ShopCheckout checkout;
    [SerializeField] private Transform entrancePoint;
    [SerializeField] private Transform exitTurnPoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private CustomerOrder order;
    [SerializeField] private CurrencySystem currencySystem;
    [SerializeField, Min(0.1f)] private float spawnInterval = 5f;
    [SerializeField] private bool spawnOnStart = true;

    private ICustomerInventory inventory;
    private ICustomerCurrency currency;

    private void Awake()
    {
        // 일반 씬에서는 Inspector에 연결한 CurrencySystem을 손님에게 전달한다.
        currency = currencySystem;
    }

    private void Start()
    {
        if (spawnOnStart) StartCoroutine(SpawnInitialCustomers());
    }

    // 실제 인벤토리/화폐 구현이 준비되면 구성 루트에서 한 번 주입
    public void BindServices(ICustomerInventory inventoryService, ICustomerCurrency currencyService)
    {
        inventory = inventoryService;
        currency = currencyService;
    }

    public void Configure(CustomerController prefab, ShopCustomerQueue queue, ShopCheckout checkoutService, Transform entrance, Transform exitTurn, Transform exit, CustomerOrder customerOrder, float interval)
    {
        customerPrefab = prefab;
        shopQueue = queue;
        checkout = checkoutService;
        checkoutStations = new[] { new CustomerCheckoutStation(queue, checkoutService) };
        entrancePoint = entrance;
        exitTurnPoint = exitTurn;
        exitPoint = exit;
        order = customerOrder;
        spawnInterval = Mathf.Max(0.1f, interval);
    }

    public void Configure(CustomerController prefab, CustomerCheckoutStation[] stations, Transform entrance, Transform exitTurn, Transform exit, CustomerOrder customerOrder, float interval)
    {
        customerPrefab = prefab;
        checkoutStations = stations;
        shopQueue = null;
        checkout = null;
        entrancePoint = entrance;
        exitTurnPoint = exitTurn;
        exitPoint = exit;
        order = customerOrder;
        spawnInterval = Mathf.Max(0.1f, interval);
    }

    public bool SpawnOne()
    {
        if (customerPrefab == null || entrancePoint == null || exitPoint == null || PoolManager.Instance == null || !TryGetShortestStation(out ShopCustomerQueue selectedQueue, out ShopCheckout selectedCheckout))
        {
            return false;
        }

        if (!NavMesh.SamplePosition(entrancePoint.position, out NavMeshHit entranceHit, 2f, NavMesh.AllAreas))
        {
            Debug.LogWarning("Customer entrance is not close enough to the NavMesh.", this);
            return false;
        }

        CustomerController customer = PoolManager.Instance.GetPool(customerPrefab);
        customer.transform.SetPositionAndRotation(entranceHit.position, entrancePoint.rotation);
        if (customer.OnSpawned(selectedQueue, selectedCheckout, exitTurnPoint, exitPoint, order, inventory, currency))
        {
            customer.ExitCompleted += OnCustomerExitCompleted;
            customer.ExitFailed += OnCustomerExitFailed;
            return true;
        }

        PoolManager.Instance.ReturnPool(customer);
        return false;
    }

    private IEnumerator SpawnInitialCustomers()
    {
        WaitForSeconds wait = new WaitForSeconds(spawnInterval);
        while (GetCustomerCount() < GetCustomerCapacity())
        {
            SpawnOne();
            yield return wait;
        }
    }

    private void OnCustomerExitCompleted(CustomerController customer)
    {
        DespawnAndReplace(customer);
    }

    private void OnCustomerExitFailed(CustomerController customer, string reason)
    {
        DespawnAndReplace(customer);
    }

    private void DespawnAndReplace(CustomerController customer)
    {
        customer.ExitCompleted -= OnCustomerExitCompleted;
        customer.ExitFailed -= OnCustomerExitFailed;

        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnPool(customer);
        }

        SpawnOne();
    }

    private bool TryGetShortestStation(out ShopCustomerQueue selectedQueue, out ShopCheckout selectedCheckout)
    {
        selectedQueue = null;
        selectedCheckout = null;

        if (checkoutStations != null)
        {
            for (int i = 0; i < checkoutStations.Length; i++)
            {
                CustomerCheckoutStation station = checkoutStations[i];
                if (station == null || !station.IsValid || station.Queue.Count >= station.Queue.Capacity)
                {
                    continue;
                }

                if (selectedQueue == null || station.Queue.Count < selectedQueue.Count)
                {
                    selectedQueue = station.Queue;
                    selectedCheckout = station.Checkout;
                }
            }
        }

        if (selectedQueue == null && shopQueue != null && checkout != null && shopQueue.Count < shopQueue.Capacity)
        {
            selectedQueue = shopQueue;
            selectedCheckout = checkout;
        }

        return selectedQueue != null;
    }

    private int GetCustomerCount()
    {
        int count = 0;
        bool hasValidStation = false;
        if (checkoutStations != null)
        {
            for (int i = 0; i < checkoutStations.Length; i++)
            {
                CustomerCheckoutStation station = checkoutStations[i];
                if (station != null && station.IsValid)
                {
                    hasValidStation = true;
                    count += station.Queue.Count;
                }
            }
        }

        return hasValidStation ? count : shopQueue != null ? shopQueue.Count : 0;
    }

    private int GetCustomerCapacity()
    {
        int capacity = 0;
        if (checkoutStations != null)
        {
            for (int i = 0; i < checkoutStations.Length; i++)
            {
                CustomerCheckoutStation station = checkoutStations[i];
                if (station != null && station.IsValid)
                {
                    capacity += station.Queue.Capacity;
                }
            }
        }

        return capacity > 0 ? capacity : shopQueue != null && checkout != null ? shopQueue.Capacity : 0;
    }
}
