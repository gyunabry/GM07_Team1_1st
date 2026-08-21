using UnityEngine;

public class Storage : MonoBehaviour
{
    [SerializeField] private ItemInventory inventory = new();
    public ItemInventory Inventory => inventory;

    
}
