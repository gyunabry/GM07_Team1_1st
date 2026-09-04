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

    public bool TryGetFirstItem(out ItemDataSO item, out int amount)
    {
        foreach (KeyValuePair<ItemDataSO, int> entry in amounts)
        {
            if (entry.Key != null && entry.Value > 0)
            {
                item = entry.Key;
                amount = entry.Value;
                return true;
            }
        }

        item = null;
        amount = 0;
        return false;
    }

    public void Clear() { amounts.Clear(); TotalAmount = 0; }

    #region 데이터 저장 및 복구
    public InventorySaveData CaptureSaveData()
    {
        InventorySaveData result = new();

        foreach (var pair in amounts)
        {
            if (pair.Key == null || pair.Value <= 0)
            {
                continue;
            }

            result.items.Add(new ItemStackSaveData
            {
                itemId = pair.Key.ItemId,
                amount = pair.Value
            });
        }

        return result;
    }

    public bool RestoreSaveData(InventorySaveData saved, ItemDatabaseSO itemDatabase)
    {
        if (itemDatabase == null)
        {
            return false;
        }

        Clear();

        if (saved?.items == null)
        {
            return true;
        }

        bool success = true;

        foreach (ItemStackSaveData stack in saved.items)
        {
            if (stack == null || stack.amount <= 0)
            {
                continue;
            }

            if (!itemDatabase.TryGetById(stack.itemId, out ItemDataSO item))
            {
                Debug.LogWarning($"사냥 직원 소지품 ID를 찾을 수 없습니다: {stack.itemId}");

                success = false;
                continue;
            }

            amounts.TryGetValue(item, out int current);
            amounts[item] = current + stack.amount;
            TotalAmount += stack.amount;
        }

        return success;
    }
#endregion
}
