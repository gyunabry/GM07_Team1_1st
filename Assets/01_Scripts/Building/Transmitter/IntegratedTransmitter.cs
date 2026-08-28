using Unity.VisualScripting;
using UnityEngine;

public class IntegratedTransmitter : MonoBehaviour
{
    [SerializeField] private ItemInventory inventory = new();

    private PlacedBuilding placedBuilding;
    private CarrierCommandService carrierCommandService;

    public ItemInventory Inventory => inventory;

    public bool CanOperate => placedBuilding != null && placedBuilding.IsComplete;

    private void Awake()
    {
        placedBuilding = GetComponent<PlacedBuilding>();
    }

    private void OnEnable()
    {
        if (placedBuilding != null)
        {
            placedBuilding.OnConstructionCompleted += HandleConstructionCompleted;
        }

        StorageSkillRegistry.Register(inventory);
    }

    private void Start()
    {
        RegisterCarrierLogistics();
    }

    private void OnDisable()
    {
        if (placedBuilding != null)
        {
            placedBuilding.OnConstructionCompleted -= HandleConstructionCompleted;
        }

        StorageSkillRegistry.Unregister(inventory);
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

    private void HandleConstructionCompleted(PlacedBuilding building)
    {
        if (building == placedBuilding)
        {
            RegisterCarrierLogistics();
        }
    }

    private void RegisterCarrierLogistics()
    {
        if (!CanOperate)
        {
            return;
        }

        if (carrierCommandService == null)
        {
            carrierCommandService = FindFirstObjectByType<CarrierCommandService>();
        }

        carrierCommandService?.ConfigureLogistics(inventory, transform);
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
