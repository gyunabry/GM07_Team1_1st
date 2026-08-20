using TMPro;
using UnityEngine;

public class PlayerInventoryWorldUI : MonoBehaviour
{
    [SerializeField] private TMP_Text countText;

    private ItemInventory inventory;

    private void Awake()
    {
        PlayerInventory playerInventory = GetComponentInParent<PlayerInventory>();

        if (playerInventory != null) inventory = playerInventory.Inventory;
    }

    private void OnEnable()
    {
        if (inventory == null) return;

        inventory.InventoryChanged += RefreshUI;
        RefreshUI();
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.InventoryChanged -= RefreshUI;
        }
    }

    private void RefreshUI()
    {
        if (inventory == null || countText == null)
        {
            return;
        }

        countText.text = $"{inventory.TotalAmount} / {inventory.Capacity}";
    }
}
