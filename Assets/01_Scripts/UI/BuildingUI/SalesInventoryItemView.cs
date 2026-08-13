using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SalesInventoryItemView : MonoBehaviour
{
    [Header("아이템 정보")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text amountText;

    public void Bind(InventoryEntry entry)
    {
        if (entry == null) return;

        ItemDataSO item = entry.Item;
        int amount = Mathf.Max(0, entry.Amount);

        bool hasIcon = item != null && item.Icon != null;

        itemIcon.sprite = item.Icon;
        itemIcon.enabled = hasIcon;

        amountText.text = $"x {amount}";
    }

    // 해당 슬롯을 비우는 메서드
    public void SetEmpty()
    {
        itemIcon.sprite = null;
        itemIcon.enabled = false;
        amountText.text = string.Empty;
    }
}
