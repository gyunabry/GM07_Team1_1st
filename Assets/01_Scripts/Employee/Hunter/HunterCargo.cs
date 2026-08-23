using System.Collections.Generic;
using UnityEngine;

/// <summary>스킬로 최대 소지량이 변하는 사냥 직원 전용 소지품입니다.</summary>
public sealed class HunterCargo
{
    private readonly Dictionary<ItemDataSO, int> amounts = new();
    public int TotalAmount { get; private set; }
    public int Capacity { get; private set; }
    public int Remaining => Mathf.Max(0, Capacity - TotalAmount);

    public void SetCapacity(int value) => Capacity = Mathf.Max(1, value);
    public int Add(ItemDataSO item, int amount)
    {
        if (item == null || amount <= 0) return 0;
        int added = Mathf.Min(amount, Remaining);
        if (added <= 0) return 0;
        amounts[item] = amounts.TryGetValue(item, out int current) ? current + added : added;
        TotalAmount += added;
        return added;
    }

    public void TransferTo(ItemInventory inventory)
    {
        if (inventory == null) return;
        List<ItemDataSO> items = new(amounts.Keys);
        foreach (ItemDataSO item in items)
        {
            int moved = inventory.Add(item, amounts[item]);
            if (moved <= 0) continue;
            amounts[item] -= moved;
            TotalAmount -= moved;
            if (amounts[item] <= 0) amounts.Remove(item);
        }
    }

    public void Clear() { amounts.Clear(); TotalAmount = 0; }
}
