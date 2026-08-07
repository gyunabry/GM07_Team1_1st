using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class InventoryEntry
{
    [SerializeField] private ItemDataSO item;
    [SerializeField, Min(0)] private int amount;

    public ItemDataSO Item => item;
    public int Amount => amount;
    public bool IsEmpty => item == null || amount <= 0;

    public InventoryEntry(ItemDataSO item, int amount)
    {
        this.item = item;
        this.amount = Mathf.Max(0, amount);
    }

    internal void Add(int value) => amount += value;

    internal int Remove(int value)
    {
        int removed = Mathf.Min(amount, value);
        amount -= removed;
        return removed;
    }
}

[Serializable]
public sealed class ItemInventory
{
    [SerializeField, Min(0)] private int capacity = 20;
    [SerializeField] private List<InventoryEntry> entries = new();

    public int Capacity => capacity;

    public int TotalAmount
    {
        get
        {
            int total = 0;

            foreach (InventoryEntry entry in entries)
            {
                if (entry != null && !entry.IsEmpty)
                {
                    total += entry.Amount;
                }
            }

            return total;
        }
    }

    public int RemainingCapacity => Mathf.Max(0, Capacity - TotalAmount);
    public IReadOnlyList<InventoryEntry> Entries => entries;

    public event Action InventoryChanged;

    public int GetAmount(ItemDataSO item)
    {
        if (item == null) return 0;

        InventoryEntry entry = FindEntry(item);
        return entry?.Amount ?? 0;
    }

    public bool Contains(ItemDataSO item, int amount)
    {
        return item != null && amount > 0 && GetAmount(item) >= amount;
    }

    public int Add(ItemDataSO item, int amount)
    {
        int added = AddInternal(item, amount);

        if (added > 0)
        {
            InventoryChanged?.Invoke();
        }

        return added;
    }

    public int Remove(ItemDataSO item, int amount)
    {
        int removed = RemoveInternal(item, amount);

        if (removed > 0)
        {
            InventoryChanged?.Invoke();
        }

        return removed;
    }

    // 출발지 보유량과 목적지 여유 공간이 허용하는 만큼 이동
    public int TransferTo(ItemInventory target, ItemDataSO item, int amount)
    {
        if (target == null || target == this || item == null || amount <= 0)
        {
            return 0;
        }

        int movable = Mathf.Min(amount, GetAmount(item));
        movable = Mathf.Min(movable, target.RemainingCapacity);

        if (movable <= 0)
        {
            return 0;
        }

        RemoveInternal(item, movable);
        target.AddInternal(item, movable);

        InventoryChanged?.Invoke();
        target.InventoryChanged?.Invoke();

        return movable;
    }

    private int AddInternal(ItemDataSO item, int amount)
    {
        if (item == null || amount <= 0) return 0;

        int added = Mathf.Min(amount, RemainingCapacity);
        if (added <= 0) return 0;

        InventoryEntry entry = FindEntry(item);

        if (entry == null)
        {
            entries.Add(new InventoryEntry(item, added));
        }
        else
        {
            entry.Add(added);
        }

        return added;
    }

    private int RemoveInternal(ItemDataSO item, int amount)
    {
        if (item == null || amount <= 0) return 0;

        InventoryEntry entry = FindEntry(item);
        if (entry == null) return 0;

        int removed = entry.Remove(amount);

        if (entry.IsEmpty)
        {
            entries.Remove(entry);
        }

        return removed;
    }

    private InventoryEntry FindEntry(ItemDataSO item)
    {
        return entries.Find(entry =>
            entry?.Item != null &&
            (entry.Item == item || entry.Item.ItemId == item.ItemId));
    }
}
