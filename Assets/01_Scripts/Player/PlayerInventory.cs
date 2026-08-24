using UnityEngine;

public class PlayerInventory : MonoBehaviour, IInventoryProvider
{
    [SerializeField] private ItemInventory inventory = new();

    public ItemInventory Inventory => inventory;

    public int GetAmount(ItemDataSO item)
    {
        return Inventory.GetAmount(item);
    }
}
