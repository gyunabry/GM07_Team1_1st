using UnityEngine;

public class PlayerInventory : MonoBehaviour, IInventoryProvider
{
    [SerializeField] private ItemInventory inventory = new();

    [SerializeField] private Transform transferAnchor;

    public ItemInventory Inventory => inventory;

    public Transform TransferAnchor => transferAnchor;

    public int GetAmount(ItemDataSO item)
    {
        return Inventory.GetAmount(item);
    }
}
