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

    // 아이템 목록을 받아서 모두 제거 시도
    public bool TryRemoveAll(IReadOnlyList<ItemAmount> items)
    {
        if (items == null || items.Count <= 0) return false;

        for (int i = 0; i < items.Count; i++)
        {
            ItemAmount item = items[i];

            if (!item.IsValid || !Contains(item.Item, item.Amount))
            {
                return false;
            }
        }

        // 모두 충분할 때만 실제 차감
        for (int i = 0; i < items.Count; i++)
        {
            RemoveInternal(items[i].Item, items[i].Amount);
        }

        InventoryChanged?.Invoke();

        return true;
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

    // 아이템이 존재하지 않거나 추가할 수량이 0 이하이면 0 반환
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

    // 아이템이 존재하지 않거나 제거할 수량이 0 이하이면 0 반환
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

    // 아이템이 같은지 비교
    // 슬롯별 최대 수량이 정해져 있기 때문에 같은 아이템이 여러 슬롯에 있을 수 있음
    private static bool IsSameItem(ItemDataSO left, ItemDataSO right)
    {
        if (left == null || right == null) return false;

        if (left == right) return true;

        // 아이템 ID가 같으면 같은 아이템으로 간주
        return !string.IsNullOrEmpty(left.ItemId) &&
            left.ItemId == right.ItemId;
    }

    // 아이템이 같은지 비교 후, 같은 아이템이 여러 슬롯에 있을 수 있으므로 첫 번째 슬롯만 반환
    private InventoryEntry FindEntry(ItemDataSO item)
    {
        return entries.Find(entry =>
            entry?.Item != null &&
            !entry.IsEmpty &&
            IsSameItem(entry.Item, item));
    }
}
