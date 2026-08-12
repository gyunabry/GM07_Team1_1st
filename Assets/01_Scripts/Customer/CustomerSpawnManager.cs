using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// 설정된 간격으로 PoolManager에서 손님을 대여해 하나의 상점 대기열에 참가
public sealed class CustomerSpawnManager : MonoBehaviour
{
    [SerializeField] private CustomerController customerPrefab;
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
        entrancePoint = entrance;
        exitTurnPoint = exitTurn;
        exitPoint = exit;
        order = customerOrder;
        spawnInterval = Mathf.Max(0.1f, interval);
    }

    public bool SpawnOne()
    {
        if (customerPrefab == null || shopQueue == null || checkout == null || entrancePoint == null || exitPoint == null || PoolManager.Instance == null)
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
        if (customer.OnSpawned(shopQueue, checkout, exitTurnPoint, exitPoint, order, inventory, currency))
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
        while (shopQueue != null && shopQueue.Count < shopQueue.Capacity)
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
}
