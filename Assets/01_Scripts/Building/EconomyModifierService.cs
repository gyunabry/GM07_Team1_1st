using System;
using System.Collections.Generic;
using UnityEngine;

public class EconomyModifierService : MonoBehaviour
{
    [SerializeField] private float discountRatio; // 시설 배치 비용 감소 비율
    [SerializeField] private float globalProductBonusRatio; // 모든 생산품 판매 가격 증가 비율

    // 정제/가열 생산품에 각각 적용
    private readonly Dictionary<ProcessType, float> processBonusRatios = new();

    public event Action CostChanged;
    public event Action PriceChanged;

    #region 시설 가격
    public int GetBuildCost(BuildingDataSO data)
    {
        if (data == null) return 0;

        return Mathf.Max(0, Mathf.CeilToInt(data.BuildCost * (1f - discountRatio)));
    }

    public void AddDiscount(float value)
    {
        discountRatio = Mathf.Clamp01(discountRatio + value);
        CostChanged?.Invoke();
    }

    public void ResetDiscount()
    {
        discountRatio = 0f;
        CostChanged?.Invoke();
    }
    #endregion

    #region 생산 아이템
    public int GetSellPrice(ItemDataSO item)
    {
        if (item == null) return 0;

        float totalBonusRatio = 0f;

        // 생산품에만 적용
        if (item.ItemType == ItemType.Product)
        {
            totalBonusRatio += globalProductBonusRatio;
            totalBonusRatio += GetProcessBonusRatio(item.ProcessType);
        }

        return Mathf.Max(0, Mathf.RoundToInt(item.SellPrice * (1f + totalBonusRatio)));
    }

    public float GetProcessBonusRatio(ProcessType processType)
    {
        return processBonusRatios.TryGetValue(processType, out float ratio) ? ratio : 0f;
    }

    public void AddGlobalProductBonusRatio(float value)
    {
        globalProductBonusRatio = Mathf.Max(0f, globalProductBonusRatio + value);

        PriceChanged?.Invoke();
    }

    public void AddProcessBonusRatio(ProcessType processType, float value)
    {
        float currentRatio = GetProcessBonusRatio(processType);

        processBonusRatios[processType] = Mathf.Max(0f, currentRatio + value);

        PriceChanged?.Invoke();
    }

    public int CalculateOrderPrice(IReadOnlyList<CustomerOrderItem> items)
    {
        if (items == null) return 0;

        int totalPrice = 0;

        foreach (CustomerOrderItem orderItem in items)
        {
            totalPrice += GetSellPrice(orderItem.ItemId) * orderItem.Amount;
        }

        return totalPrice;
    }

    public void ResetBonusRatio()
    {
        globalProductBonusRatio = 0f;
        processBonusRatios.Clear();

        PriceChanged?.Invoke();
    }
    #endregion
}
