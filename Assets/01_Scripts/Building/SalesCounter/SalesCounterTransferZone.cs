using System.Collections;
using UnityEngine;

public class SalesCounterTransferZone : MonoBehaviour
{
    [Header("전송 설정")]
    [SerializeField] private SalesCounter salesCounter;
    [SerializeField] private float transferInterval = 0.1f;
    [SerializeField] private LayerMask playerLayer;

    [Header("전송 효과")]
    [SerializeField] private ItemTransferEffect transferEffectPrefab;
    [SerializeField] private Transform transferAnchor;

    private PlayerInventory currentPlayer;
    private Coroutine transferCoroutine;
    private WaitForSeconds transferWait;

    private void Awake()
    {
        if (salesCounter == null)
        {
            salesCounter = GetComponentInParent<SalesCounter>();
        }

        if (salesCounter == null)
        {
            enabled = false;
            return;
        }

        transferWait = new WaitForSeconds(transferInterval);
    }

    private void OnDisable()
    {
        StopTransfer();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayerLayer(other.gameObject.layer)) return;

        PlayerInventory player = other.GetComponentInParent<PlayerInventory>();

        if (player == null) return;

        currentPlayer = player;

        if (transferCoroutine == null)
        {
            transferCoroutine = StartCoroutine(TransferCo());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (currentPlayer != null)
        {
            StopTransfer();
        }
    }

    private IEnumerator TransferCo()
    {
        while (currentPlayer != null)
        {
            TryTransferOne(currentPlayer);
            yield return transferWait;
        }

        transferCoroutine = null;
    }

    
    private int TryTransferOne(PlayerInventory player)
    {
        if (player == null || salesCounter == null || !salesCounter.CanOperate)
        {
            return 0;
        }

        ItemInventory targetInventory = salesCounter.Inventory;

        if (targetInventory == null || targetInventory.RemainingCapacity <= 0)
        {
            return 0;
        }

        ItemDataSO product = FindFirstProduct(player.Inventory);

        if (product == null) return 0;

        int moved = player.Inventory.TransferTo(targetInventory, product, 1);

        if (moved > 0)
        {
            Transform destination = transferAnchor != null ? transferAnchor : transform;

            ItemTransferEffect.Play(
                transferEffectPrefab,
                product.Icon,
                player.TransferAnchor.position,    // 생산 시설 출발
                destination
            );     // 플레이어 도착
        }

        return moved;
    }

    private static ItemDataSO FindFirstProduct(ItemInventory playerInventory)
    {
        if (playerInventory == null) return null;

        foreach (InventoryEntry entry in playerInventory.Entries)
        {
            if (entry == null || entry.IsEmpty || entry.Item == null) continue;

            if (entry.Item.ItemType == ItemType.Product)
            {
                return entry.Item;
            }
        }

        return null;
    }

    private bool IsPlayerLayer(int layer)
    {
        return (playerLayer.value & (1 << layer)) != 0;
    }

    private void StopTransfer()
    {
        if (transferCoroutine != null)
        {
            StopCoroutine(transferCoroutine);
            transferCoroutine = null;
        }

        currentPlayer = null;
    }
}
