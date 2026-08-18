using Unity.VisualScripting;
using UnityEngine;

public class IntegratedTransmitter : MonoBehaviour
{
    [SerializeField] private ItemInventory inventory = new();

    private PlacedBuilding placedBuilding;

    public ItemInventory Inventory => inventory;

    public bool CanOperate => placedBuilding != null && placedBuilding.IsComplete;

    private void Awake()
    {
        placedBuilding = GetComponent<PlacedBuilding>();
    }

    public int TryGiveOne(ItemInventory targetInventory)
    {
        if (!CanOperate || 
            inventory == null || 
            targetInventory == null ||
            targetInventory.RemainingCapacity <= 0)
        {
            return 0;
        }

        ItemDataSO material = FindFirstMaterial();
        if (material == null) return 0;

        return inventory.TransferTo(targetInventory, material, 1);
    }

    private ItemDataSO FindFirstMaterial()
    {
        foreach (InventoryEntry entry in inventory.Entries)
        {
            if (entry == null || entry.IsEmpty || entry.Item == null) continue;

            if (entry.Item.ItemType == ItemType.Material)
            {
                return entry.Item;
            }
        }

        return null;
    }
}
