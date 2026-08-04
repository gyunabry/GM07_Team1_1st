using System.Collections;
using UnityEngine;

// 설정된 간격으로 PoolManager에서 손님을 대여해 하나의 상점 대기열에 참가
public sealed class CustomerSpawnManager : MonoBehaviour
{
    [SerializeField] private CustomerController customerPrefab;
    [SerializeField] private ShopCustomerQueue shopQueue;
    [SerializeField] private Transform entrancePoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private CustomerOrder order;
    [SerializeField, Min(0.1f)] private float spawnInterval = 5f;
    [SerializeField] private bool spawnOnStart = true;

    private ICustomerInventory inventory;
    private ICustomerCurrency currency;

    private void Start()
    {
        if (spawnOnStart) StartCoroutine(SpawnRoutine());
    }

    // 실제 인벤토리/화폐 구현이 준비되면 구성 루트에서 한 번 주입
    public void BindServices(ICustomerInventory inventoryService, ICustomerCurrency currencyService)
    {
        inventory = inventoryService;
        currency = currencyService;
    }

    public bool SpawnOne()
    {
        if (customerPrefab == null || shopQueue == null || entrancePoint == null || exitPoint == null || PoolManager.Instance == null)
        {
            return false;
        }

        CustomerController customer = PoolManager.Instance.GetPool(customerPrefab);
        customer.transform.SetPositionAndRotation(entrancePoint.position, entrancePoint.rotation);
        if (customer.OnSpawned(shopQueue, exitPoint, order, inventory, currency)) return true;

        PoolManager.Instance.ReturnPool(customer);
        return false;
    }

    private IEnumerator SpawnRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(spawnInterval);
        while (true) { SpawnOne(); yield return wait; }
    }
}
