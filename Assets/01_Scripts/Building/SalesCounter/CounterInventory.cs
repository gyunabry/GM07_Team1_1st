using System;
using System.Collections.Generic;
using UnityEngine;

public class CounterInventory : MonoBehaviour, ICustomerInventory
{
    [SerializeField] private ItemInventory inventory = new();

    public ItemInventory Inventory => inventory;

    // 모든 판매대가 하나의 인벤토리를 공유하도록 싱글톤 패턴 적용
    public static CounterInventory Instance { get; private set; }

    public event Action InventoryChanged
    {
        add => inventory.InventoryChanged += value;
        remove => inventory.InventoryChanged -= value;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool TryConsumeAll(IReadOnlyList<CustomerOrderItem> items)
    {
        if (items == null || items.Count <= 0) return false;

        List<ItemAmount> requirements = new List<ItemAmount>(items.Count);

        for (int i = 0; i < items.Count; i++)
        {
            CustomerOrderItem orderItem = items[i];

            if (!orderItem.IsValid)
            {
                return false;
            }

            requirements.Add(new ItemAmount(orderItem.ItemId, orderItem.Amount));
        }

        return inventory.TryRemoveAll(requirements);
    }
}
