using System.Collections;
using UnityEngine;

/*
1. 상호작용 영역에 플레이어나 사냥 직원이 접근하고 전송 가능한 재료 아이템이 있다면 전송
2. 공방 내부에 배치되는 통합 전송기 인벤토리로 이동됨
3. 이때, 전송기의 아이템은 일정시간마다 하나씩 통합 전송기로 전송
4. 통합 전송기는 플레이어가 근접하면 플레이어 인벤토리로 아이템을 이동시킴
5. 각 전송기의 종착지는 통합 전송기
*/

// 플레이어와 사냥 직원이 이용할 전송기 클래스
public class Transmitter : MonoBehaviour
{
    [Header("전송기 인벤토리")]
    [SerializeField] private ItemInventory inventory = new();

    [Header("전송 설정")]
    [SerializeField] private IntegratedTransmitter destination;
    [SerializeField] private Transform depositPoint;
    [SerializeField] private float transferInterval = 1f;

    private PlacedBuilding placedBuilding;
    private Coroutine transferCoroutine;
    private WaitForSeconds transferWait;

    public ItemInventory Inventory => inventory;
    public bool CanOperate => placedBuilding != null && placedBuilding.IsComplete;
    public Transform DepositPoint => depositPoint;


    private void Awake()
    {
        placedBuilding = GetComponent<PlacedBuilding>();

        transferWait = new WaitForSeconds(transferInterval);
    }

    private void Start()
    {
        if (destination == null)
        {
            destination = FindAnyObjectByType<IntegratedTransmitter>();
        }
    }

    private void OnEnable()
    {
        if (transferCoroutine == null)
        {
            transferCoroutine = StartCoroutine(TransferCo());
        }
    }

    private void OnDisable()
    {
        if (transferCoroutine == null) return;

        StopCoroutine(transferCoroutine);
        transferCoroutine = null;
    }

    // 플레이어, 사냥 직원의 인벤토리에서 호출해 재료를 받는 메서드
    public int TryReceiveOne(ItemInventory sourceInventory)
    {
        if (!CanOperate || 
            sourceInventory == null || 
            inventory == null || 
            inventory.RemainingCapacity <= 0)
        {
            return 0;
        }

        ItemDataSO material = FindFirstMaterial(sourceInventory);

        if (material == null) return 0;

        return sourceInventory.TransferTo(inventory, material, 1);
    }

    private IEnumerator TransferCo()
    {
        while (true)
        {
            yield return transferWait;

            TryTrasferOne();
        }
    }

    private int TryTrasferOne()
    {
        if (!CanOperate || destination == null || !destination.CanOperate)
        {
            return 0;
        }

        ItemInventory destinationInventory = destination.Inventory;

        // 목적지 인벤토리의 수용량이 남아있지 않다면 0 반환
        if (destinationInventory == null || destinationInventory.RemainingCapacity <= 0)
        {
            return 0;
        }

        ItemDataSO material = FindFirstMaterial(inventory);
        if (material == null) return 0;

        return inventory.TransferTo(destinationInventory, material, 1);
    }

    private static ItemDataSO FindFirstMaterial(ItemInventory sourceInventory)
    {
        if (sourceInventory == null) return null;

        foreach (InventoryEntry entry in sourceInventory.Entries)
        {
            if (entry == null || entry.IsEmpty || entry.Item == null) return null;

            if (entry.Item.ItemType == ItemType.Material)
            {
                return entry.Item;
            }
        }

        return null;
    }
}
