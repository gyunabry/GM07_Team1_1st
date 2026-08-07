using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private ItemInventory inventory = new();

    [Header("테스트")]
    [SerializeField] private ItemDataSO item;
    [SerializeField] private int amount = 10;

    public ItemInventory Inventory => inventory;

    public void GiveItem(ItemDataSO item, int amount)
    {
        int added = inventory.Add(item, amount);
        Debug.Log($"플레이어에게 {item.ItemName} 지급");
    }
    public int GetAmount(ItemDataSO item)
    {
        return Inventory.GetAmount(item);
    }
}
