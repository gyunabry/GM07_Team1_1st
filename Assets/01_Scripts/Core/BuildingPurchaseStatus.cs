using System;
using UnityEngine;

// 구매 가능 여부를 검사하는 partial 클래스

[Flags]
public enum PurchaseBlockReason
{
    None = 0,
    Level = 1 << 0,
    Money = 1 << 1,
    PlacementLimit = 1 << 2,
}

public readonly struct BuildingPurchaseStatus
{
    public readonly int FinalCost;
    public readonly int CurrentCount;
    public readonly int MaxCount;
    public readonly PurchaseBlockReason BlockReasons;

    public bool CanPurchase => BlockReasons == PurchaseBlockReason.None;

    public BuildingPurchaseStatus(
        int finalCost,
        int currentCount,
        int maxCount,
        PurchaseBlockReason blockReasons)
    {
        FinalCost = finalCost;
        CurrentCount = currentCount;
        MaxCount = maxCount;
        BlockReasons = blockReasons;
    }
}

public partial class PlacementSystem : MonoBehaviour
{
    [SerializeField] private Player player;

    public BuildingPurchaseStatus EvaluatePurchase(BuildingDataSO data)
    {
        if (data == null) return default;

        int finalCost = economyModifier.GetBuildCost(data);
        int currentCount = FacilityManager.Instance.GetPlacedCount(data);
        int playerLevel = player != null ? player.NowLevel : 0;
        int currentMoney = CurrencySystem.Instance.Money;

        PurchaseBlockReason reasons = PurchaseBlockReason.None;

        if (playerLevel < data.RequiredLevel)
        {
            reasons |= PurchaseBlockReason.Level;
        }

        if (currentMoney < finalCost)
        {
            reasons |= PurchaseBlockReason.Money;
        }

        if (currentCount >= data.PlacementLimit)
        {
            reasons |= PurchaseBlockReason.PlacementLimit;
        }

        return new BuildingPurchaseStatus(
            finalCost,
            currentCount,
            data.PlacementLimit,
            reasons
        );
    }
}
