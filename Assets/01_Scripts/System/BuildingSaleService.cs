using UnityEngine;

// 시설 판매 시 판매 가능 여부, 환불액, 재고 이동량을 관리할 클래스
// 재료는 통합 전송기, 생산품은 판매대 인벤토리로 이동

public readonly struct BuildingSaleEvaluation
{
    public bool CanSell { get; }
    public int Refund { get; }

    public BuildingSaleEvaluation(
        bool canSell,
        int refund)
    {
        CanSell = canSell;
        Refund = refund;
    }
 }

public class BuildingSaleService : MonoBehaviour
{
    [SerializeField] private float refundRatio = 0.7f;

    public BuildingSaleEvaluation Evaluate(PlacedBuilding building)
    {
        if (building == null || building.Data == null)
        {
            return new BuildingSaleEvaluation(false, 0);
        }

        if (building.Data.Sellable != SellableType.Possible)
        {
            return new BuildingSaleEvaluation(false, 0);
        }

        int refund = Mathf.RoundToInt(building.Data.BuildCost * refundRatio);

        return new BuildingSaleEvaluation(true, refund);
    }

    // 생산 시설 판매 시 재료는 통합 전송기로, 생산품은 판매대로 즉시 전송
    public void TransferInventoryForSale(PlacedBuilding building)
    {
        if (building == null || !building.TryGetComponent(out ProductionBuilding productionBuilding))
        {
            return;
        }

        IntegratedTransmitter transmitter = FindAnyObjectByType<IntegratedTransmitter>();
        if (transmitter == null) return;
        ItemInventory materialStorage = transmitter.Inventory;

        ItemInventory salesInventory = 
            CounterInventory.Instance != null ? 
            CounterInventory.Instance.Inventory : 
            null;

        TransferAllPossible(productionBuilding.InputInventory, materialStorage);
        TransferAllPossible(productionBuilding.OutputInventory, salesInventory);

    }

    // 상한을 넘는 아이템은 우선 삭제
    private static void TransferAllPossible(ItemInventory source, ItemInventory target)
    {
        if (source == null || target == null) return;

        for (int i = source.Entries.Count - 1; i >= 0; i++)
        {
            InventoryEntry entry = source.Entries[i];

            if (entry == null || entry.IsEmpty || entry.Item == null)
            {
                continue;
            }

            source.TransferTo(target, entry.Item, entry.Amount);
        }
    }
}
