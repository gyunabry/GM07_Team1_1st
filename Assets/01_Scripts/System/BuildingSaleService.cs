using UnityEngine;

// 시설 판매 시 판매 가능 여부, 환불액, 재고 이동량을 관리할 클래스
// 재료는 통합 전송기, 생산품은 판매대 인벤토리로 이동

public readonly struct BuildingSaleEvaluation
{
    public bool CanSell { get; }
    public int Refund { get; }
    public bool HasItemToDiscard { get; } // 아이템 소실 여부

    public BuildingSaleEvaluation(
        bool canSell,
        int refund,
        bool hasItemsToDiscard)
    {
        CanSell = canSell;
        Refund = refund;
        HasItemToDiscard = hasItemsToDiscard;
    }
 }

public class BuildingSaleService : MonoBehaviour
{
    [SerializeField] private float refundRatio = 0.7f;

    public BuildingSaleEvaluation Evaluate(PlacedBuilding building)
    {
        // 시설 데이터가 없을 때
        if (building == null || building.Data == null)
        {
            return new BuildingSaleEvaluation(false, 0, false);
        }

        SellableType sellableType = building.Data.Sellable;

        // 판매 가능한 시설과 부분적 판매가 가능한 시설은 2개 이상일때 판매 가능
        bool canSell =
            sellableType == SellableType.Possible ||
            (sellableType == SellableType.Patial &&
            FacilityManager.Instance != null &&
            FacilityManager.Instance.GetPlacedCount(building.Data) >= 2);

        if (!canSell)
        {
            return new BuildingSaleEvaluation(false, 0, false);
        }

        int refund = Mathf.RoundToInt(building.Data.BuildCost * refundRatio);
        bool hasItemsToDiscard = false;

        // 통합 전송기, 판매대 여유 공간 검사
        if (building.TryGetComponent(out ProductionBuilding production))
        {
            ItemInventory materialInventory = FindMaterialInventory();
            ItemInventory salesInventory = FindSalesIntentory();

            hasItemsToDiscard =
                HasUntrasferItems(production.InputInventory, materialInventory) ||
                HasUntrasferItems(production.OutputInventory, salesInventory);
        }

        return new BuildingSaleEvaluation(true, refund, hasItemsToDiscard);
    }

    // 생산 시설 판매 시 재료는 통합 전송기로, 생산품은 판매대로 즉시 전송
    // 나머지 아이템은 월드에 드랍
    public void TransferInventoryForSale(PlacedBuilding building)
    {
        if (building == null || !building.TryGetComponent(out ProductionBuilding productionBuilding))
        {
            return;
        }

        productionBuilding.enabled = false;

        ItemInventory materialInventory = FindMaterialInventory();
        ItemInventory salesInventory = FindSalesIntentory();
        Vector3 dropPosition = building.transform.position;

        // 가능한 수량까지만 전송
        TransferAllOrDrop(
            productionBuilding.InputInventory,
            materialInventory,
            dropPosition);

        TransferAllOrDrop(
            productionBuilding.OutputInventory,
            salesInventory,
            dropPosition);
    }

    public static void TransferAllOrDrop(ItemInventory source, ItemInventory target, Vector3 dropPosition)
    {
        if (source == null) return;

        source.TransferAllTo(target);

        DropRemainingItems(source, dropPosition);
    }

    private static void DropRemainingItems(ItemInventory source, Vector3 pos)
    {
        PoolManager poolManager = PoolManager.Instance;

        if (source == null || source.TotalAmount <= 0 || poolManager == null)
        {
            return;
        }

        for (int i = source.Entries.Count - 1; i >= 0; i--)
        {
            InventoryEntry entry = source.Entries[i];

            if (entry == null || entry.IsEmpty || entry.Item == null)
            {
                continue;
            }

            ItemDataSO item = entry.Item;
            int amount = entry.Amount;

            Dropitem droppedItem = poolManager.GetPool<Dropitem>();

            if (droppedItem == null)
            {
                Debug.LogWarning("DropItem 풀이 등록되지 않았습니다.");
                return;
            }

            droppedItem.transform.position = pos;
            droppedItem.Initialize(item, amount);

            source.Remove(item, amount);
        }
    }

    private ItemInventory FindMaterialInventory()
    {
        IntegratedTransmitter transmitter = FindAnyObjectByType<IntegratedTransmitter>();

        return transmitter != null ? transmitter.Inventory : null;
    }

    private ItemInventory FindSalesIntentory()
    {
        return CounterInventory.Instance != null ? CounterInventory.Instance.Inventory : null;
    }

    // 전송 대상 인벤토리에 여유 공간이 있는지 검사
    private bool HasUntrasferItems(ItemInventory source, ItemInventory target)
    {
        if (source == null || source.TotalAmount <= 0)
        {
            return false;
        }

        return !source.CanTransferAllTo(target);
    }
}
