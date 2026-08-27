using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// 활성 계산대 목록을 관리하고 가장 짧은 줄로 손님을 배정한다.
public sealed class CustomerSpawnManager : MonoBehaviour
{
    [SerializeField] private CustomerController customerPrefab;
    [SerializeField] private GameObject[] customerVisualPrefabs;
    [SerializeField] private Transform entrancePoint;
    [SerializeField] private Transform exitTurnPoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private CurrencySystem currencySystem;
    [SerializeField] private CounterInventory counterInventory;
    [SerializeField, Min(0.1f)] private float spawnInterval = 5f;
    [SerializeField] private bool spawnOnStart = true;

    private readonly HashSet<CustomerCheckoutStation> stations = new HashSet<CustomerCheckoutStation>();
    private readonly Dictionary<CustomerController, GameObject[]> customerVisualInstances = new Dictionary<CustomerController, GameObject[]>();
    private ICustomerInventory inventory;
    private ICustomerCurrency currency;
    private Coroutine replenishRoutine;
    private float patienceBonusSeconds;
    private float patienceBonusPercent;
    private float visitIntervalReductionPercent;

    public static CustomerSpawnManager Instance { get; private set; }
    public float PatienceBonusSeconds => patienceBonusSeconds;
    public float PatienceBonusPercent => patienceBonusPercent;
    public float VisitIntervalReductionPercent => visitIntervalReductionPercent;
    public float VisitInterval => Mathf.Max(0.1f, spawnInterval * (1f - visitIntervalReductionPercent / 100f));

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Only one CustomerSpawnManager can be active.", this);
            enabled = false;
            return;
        }

        Instance = this;
        currency = currencySystem;
        inventory = counterInventory;
    }

    private void Start()
    {
        if (spawnOnStart)
        {
            RequestReplenishment();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void BindServices(ICustomerInventory inventoryService, ICustomerCurrency currencyService)
    {
        inventory = inventoryService;
        currency = currencyService;
    }

    public void AddCustomerPatienceSeconds(float seconds)
    {
        if (seconds <= 0f)
        {
            return;
        }

        SetCustomerPatienceBonusSeconds(patienceBonusSeconds + seconds);
    }

    public void SetCustomerPatienceBonusSeconds(float seconds)
    {
        patienceBonusSeconds = Mathf.Max(0f, seconds);

        foreach (CustomerCheckoutStation station in stations)
        {
            if (station?.Queue == null)
            {
                continue;
            }

            CustomerController[] customers = station.Queue.GetCustomersSnapshot();
            for (int i = 0; i < customers.Length; i++)
            {
                customers[i]?.SetPatienceBonusSeconds(patienceBonusSeconds);
            }
        }
    }

    // 스킬 효과가 레벨별 최종 인내심 증가율(예: 5, 10)을 전달한다.
    public void SetPatienceIncreasePercent(float percent)
    {
        patienceBonusPercent = Mathf.Max(0f, percent);

        foreach (CustomerCheckoutStation station in stations)
        {
            if (station?.Queue == null)
            {
                continue;
            }

            CustomerController[] customers = station.Queue.GetCustomersSnapshot();
            for (int i = 0; i < customers.Length; i++)
            {
                customers[i]?.SetPatienceBonusPercent(patienceBonusPercent);
            }
        }
    }

    // 스킬 효과가 레벨별 최종 방문 간격 감소율(예: 5, 10)을 전달한다.
    public void SetSpawnIntervalReductionPercent(float percent)
    {
        visitIntervalReductionPercent = Mathf.Clamp(percent, 0f, 100f);
    }

    // 전역 입·출구와 주문만 구성한다. 계산대는 CustomerCheckoutStation이 자동 등록한다.
    public void Configure(CustomerController prefab, Transform entrance, Transform exitTurn, Transform exit, float interval)
    {
        customerPrefab = prefab;
        entrancePoint = entrance;
        exitTurnPoint = exitTurn;
        exitPoint = exit;
        spawnInterval = Mathf.Max(0.1f, interval);
    }

    public void RegisterStation(CustomerCheckoutStation station)
    {
        if (station == null || station.Queue == null || station.Checkout == null)
        {
            Debug.LogWarning("Customer checkout station requires a queue and checkout.", station);
            return;
        }

        stations.Add(station);
        station.SetOpen(true);
        station.Queue.SetAcceptingCustomers(true);
        RequestReplenishment();
    }

    public void CloseStation(CustomerCheckoutStation station)
    {
        if (station == null || !stations.Contains(station))
        {
            return;
        }

        station.SetOpen(false);
        ForceExitStationCustomers(station);
    }

    public void UnregisterStation(CustomerCheckoutStation station)
    {
        if (station == null)
        {
            return;
        }

        station.SetOpen(false);
        ForceExitStationCustomers(station);
        stations.Remove(station);
    }

    public bool SpawnOne()
    {
        if (customerPrefab == null 
            || entrancePoint == null 
            || exitPoint == null 
            || PoolManager.Instance == null 
            || !TryGetShortestStation(out CustomerCheckoutStation station))
        {
            return false;
        }

        if (!NavMesh.SamplePosition(entrancePoint.position, out NavMeshHit entranceHit, 2f, NavMesh.AllAreas))
        {
            Debug.LogWarning("Customer entrance is not close enough to the NavMesh.", this);
            return false;
        }

        CustomerOrder order = CreateOrder();
        if (!order.IsValid)
        {
            Debug.LogWarning("Customer spawn skipped because no valid customer order is available.", this);
            return false;
        }

        CustomerController customer = PoolManager.Instance.GetPool(customerPrefab);
        customer.transform.SetPositionAndRotation(entranceHit.position, entrancePoint.rotation);
        ApplyRandomVisual(customer);
        if (customer.OnSpawned(station.Queue, station.Checkout, exitTurnPoint, exitPoint, order, inventory, currency))
        {
            customer.SetPatienceBonusSeconds(patienceBonusSeconds);
            customer.SetPatienceBonusPercent(patienceBonusPercent);
            customer.ExitCompleted += OnCustomerExitCompleted;
            customer.ExitFailed += OnCustomerExitFailed;
            return true;
        }

        PoolManager.Instance.ReturnPool(customer);
        return false;
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

        RequestReplenishment();
    }

    private void ForceExitStationCustomers(CustomerCheckoutStation station)
    {
        if (station.Queue == null)
        {
            return;
        }

        station.Queue.SetAcceptingCustomers(false);
        CustomerController[] customers = station.Queue.GetCustomersSnapshot();
        for (int i = 0; i < customers.Length; i++)
        {
            customers[i]?.ForceExitWithoutPayment();
        }
    }

    private CustomerOrder CreateOrder()
    {
        CustomerOrder fallbackOrder = customerPrefab != null ? customerPrefab.DefaultOrder : default;
        RecipeUnlockManager unlockManager = RecipeUnlockManager.Instance;
        CustomerOrderGenerator generator = new CustomerOrderGenerator(unlockManager);
        if (generator.TryCreateOrder(fallbackOrder, out CustomerOrder generatedOrder))
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"Customer order generated: {generatedOrder.Items[0].ItemId.ItemId}");
#endif
            return generatedOrder;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning("Customer order fallback: no valid product candidate was found.", this);
#endif
        return fallbackOrder;
    }

    private void ApplyRandomVisual(CustomerController customer)
    {
        if (customer == null || customerVisualPrefabs == null || customerVisualPrefabs.Length == 0)
        {
            return;
        }

        int validVisualCount = 0;
        for (int i = 0; i < customerVisualPrefabs.Length; i++)
        {
            if (customerVisualPrefabs[i] != null)
            {
                validVisualCount++;
            }
        }

        if (validVisualCount == 0)
        {
            return;
        }

        int selectedVisual = Random.Range(0, validVisualCount);
        GameObject[] instances = GetOrCreateVisualInstances(customer);

        for (int i = 0, validIndex = 0; i < instances.Length; i++)
        {
            if (instances[i] == null)
            {
                continue;
            }

            instances[i].SetActive(validIndex == selectedVisual);
            validIndex++;
        }
    }

    private GameObject[] GetOrCreateVisualInstances(CustomerController customer)
    {
        if (customerVisualInstances.TryGetValue(customer, out GameObject[] instances))
        {
            return instances;
        }

        instances = new GameObject[customerVisualPrefabs.Length];
        for (int i = 0; i < customerVisualPrefabs.Length; i++)
        {
            GameObject visualPrefab = customerVisualPrefabs[i];
            if (visualPrefab == null)
            {
                continue;
            }

            GameObject visual = Instantiate(visualPrefab, customer.transform);
            visual.name = visualPrefab.name;
            visual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            visual.transform.localScale = Vector3.one;
            visual.SetActive(false);
            instances[i] = visual;
        }

        Renderer baseRenderer = customer.GetComponent<Renderer>();
        if (baseRenderer != null)
        {
            baseRenderer.enabled = false;
        }

        customerVisualInstances.Add(customer, instances);
        return instances;
    }

    private void RequestReplenishment()
    {
        if (!spawnOnStart || replenishRoutine != null || GetCustomerCapacity() <= GetCustomerCount())
        {
            return;
        }

        replenishRoutine = StartCoroutine(ReplenishCustomers());
    }

    private IEnumerator ReplenishCustomers()
    {
        while (GetCustomerCount() < GetCustomerCapacity())
        {
            if (!SpawnOne())
            {
                break;
            }

            yield return new WaitForSeconds(VisitInterval);
        }

        replenishRoutine = null;
    }

    private bool TryGetShortestStation(out CustomerCheckoutStation selectedStation)
    {
        selectedStation = null;
        foreach (CustomerCheckoutStation station in stations)
        {
            if (station == null || !station.IsAvailable || !station.Queue.IsAcceptingCustomers || station.Queue.Count >= station.Queue.Capacity)
            {
                continue;
            }

            if (selectedStation == null || station.Queue.Count < selectedStation.Queue.Count)
            {
                selectedStation = station;
            }
        }

        return selectedStation != null;
    }

    private int GetCustomerCount()
    {
        int count = 0;
        foreach (CustomerCheckoutStation station in stations)
        {
            if (station != null && station.IsAvailable)
            {
                count += station.Queue.Count;
            }
        }

        return count;
    }

    private int GetCustomerCapacity()
    {
        int capacity = 0;
        foreach (CustomerCheckoutStation station in stations)
        {
            if (station != null && station.IsAvailable)
            {
                capacity += station.Queue.Capacity;
            }
        }

        return capacity;
    }
}
