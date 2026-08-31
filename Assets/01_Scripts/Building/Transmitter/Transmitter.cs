using System.Collections;
using System.Collections.Generic;
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
    [Tooltip("동시에 물품을 맡길 직원 수만큼 지정합니다. 비어 있으면 Deposit Point 하나를 단일 슬롯으로 사용합니다.")]
    [SerializeField] private Transform[] depositSlots;
    [Header("대기열 설정")]
    [SerializeField, Min(1)] private int waitingSlotCount = 3;
    [SerializeField, Min(0.1f)] private float waitingSlotSpacing = 1f;
    [SerializeField, Min(0f)] private float waitingDistanceFromDepositPoint = 1.5f;
    [SerializeField] private float transferInterval = 1f;

    private PlacedBuilding placedBuilding;
    private Coroutine transferCoroutine;
    private WaitForSeconds transferWait;
    private readonly Dictionary<Component, Transform> depositSlotOwners = new();
    private readonly Dictionary<Component, Vector3> waitingSlotOwners = new();
    private readonly List<Component> staleDepositSlotOwners = new();

    public ItemInventory Inventory => inventory;
    public bool CanOperate => placedBuilding != null && placedBuilding.IsComplete;
    public Transform DepositPoint => depositPoint;

    /// <summary>
    /// 직원이 독점적으로 사용할 물품 전달 위치를 예약합니다.
    /// 같은 전송기 위치로 여러 NavMeshAgent가 진입하는 것을 방지합니다.
    /// </summary>
    public bool TryReserveDepositSlot(Component requester, out Transform slot)
    {
        slot = null;

        if (requester == null || !CanOperate)
        {
            return false;
        }

        CleanupSlotOwners();

        if (depositSlotOwners.TryGetValue(requester, out slot) && slot != null)
        {
            return true;
        }

        for (int i = 0; i < DepositSlotCount; i++)
        {
            Transform candidate = GetDepositSlot(i);
            if (candidate == null || depositSlotOwners.ContainsValue(candidate))
            {
                continue;
            }

            depositSlotOwners.Add(requester, candidate);
            slot = candidate;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 전송 슬롯을 기다리는 직원에게 전송기 앞의 대기 위치를 할당합니다.
    /// 별도 씬 오브젝트 없이 Deposit Point의 방향과 간격으로 위치를 계산합니다.
    /// </summary>
    public bool TryReserveWaitingSlot(Component requester, out Vector3 waitingPosition)
    {
        waitingPosition = default;

        if (requester == null || !CanOperate || depositPoint == null)
        {
            return false;
        }

        CleanupSlotOwners();

        if (waitingSlotOwners.TryGetValue(requester, out waitingPosition))
        {
            return true;
        }

        for (int i = 0; i < waitingSlotCount; i++)
        {
            Vector3 candidate = GetWaitingSlotPosition(i);
            if (IsWaitingPositionReserved(candidate))
            {
                continue;
            }

            waitingSlotOwners.Add(requester, candidate);
            waitingPosition = candidate;
            return true;
        }

        return false;
    }

    public void ReleaseDepositSlot(Component requester)
    {
        if (requester != null)
        {
            depositSlotOwners.Remove(requester);
        }
    }

    public void ReleaseWaitingSlot(Component requester)
    {
        if (requester != null)
        {
            waitingSlotOwners.Remove(requester);
        }
    }

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
        if (transferCoroutine != null)
        {
            StopCoroutine(transferCoroutine);
            transferCoroutine = null;
        }

        depositSlotOwners.Clear();
        waitingSlotOwners.Clear();
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

    private int DepositSlotCount => depositSlots != null && depositSlots.Length > 0 ? depositSlots.Length : 1;

    private Transform GetDepositSlot(int index)
    {
        return depositSlots != null && depositSlots.Length > 0 ? depositSlots[index] : depositPoint;
    }

    private Vector3 GetWaitingSlotPosition(int index)
    {
        const int slotsPerRow = 3;
        int row = index / slotsPerRow;
        int column = index % slotsPerRow - 1;
        Vector3 forward = depositPoint.forward;
        Vector3 right = depositPoint.right;

        return depositPoint.position +
               forward * (waitingDistanceFromDepositPoint + row * waitingSlotSpacing) +
               right * (column * waitingSlotSpacing);
    }

    private bool IsWaitingPositionReserved(Vector3 candidate)
    {
        foreach (Vector3 reservedPosition in waitingSlotOwners.Values)
        {
            if ((reservedPosition - candidate).sqrMagnitude < 0.01f)
            {
                return true;
            }
        }

        return false;
    }

    private void CleanupSlotOwners()
    {
        staleDepositSlotOwners.Clear();

        foreach (KeyValuePair<Component, Transform> owner in depositSlotOwners)
        {
            if (owner.Key == null || owner.Value == null)
            {
                staleDepositSlotOwners.Add(owner.Key);
            }
        }

        foreach (Component owner in staleDepositSlotOwners)
        {
            depositSlotOwners.Remove(owner);
            waitingSlotOwners.Remove(owner);
        }

        staleDepositSlotOwners.Clear();
        foreach (KeyValuePair<Component, Vector3> owner in waitingSlotOwners)
        {
            if (owner.Key == null)
            {
                staleDepositSlotOwners.Add(owner.Key);
            }
        }

        foreach (Component owner in staleDepositSlotOwners)
        {
            waitingSlotOwners.Remove(owner);
        }
    }
}
