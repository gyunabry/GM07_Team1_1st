using TMPro;
using UnityEngine;

public class PlayerInventoryWorldUI : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
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
        if (inventory == null) return;

        int totalAmount = inventory.TotalAmount;
        SetVisible(totalAmount > 0);

        if (totalAmount <= 0 || countText == null) return;

        countText.text = $"{inventory.TotalAmount} / {inventory.Capacity}";
    }

    private void SetVisible(bool visible)
    {
        if (canvas != null)
        {
            canvas.enabled = visible;
        }
    }
}
