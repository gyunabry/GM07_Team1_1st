using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SalesInventoryItemView : MonoBehaviour
{
    private Button button;
    private StorageDecomposition storageDecomposition;

    [Header("아이템 정보")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text amountText;

    public void Bind(InventoryEntry entry, StorageDecomposition SD)
    {
        if (entry == null) return;

        button = GetComponent<Button>();
        storageDecomposition = SD;
        ItemDataSO item = entry.Item;
        int amount = Mathf.Max(0, entry.Amount);

        bool hasIcon = item != null && item.Icon != null;

        itemIcon.sprite = item.Icon;
        itemIcon.enabled = hasIcon;

        if(amount > 1000)
        {
            float k = amount / 1000;
            amountText.text = $"x {k}k";
        }
        else
        {
            amountText.text = $"x {amount}";
        }

        button.onClick.AddListener(() => storageDecomposition.OnClickDecompositionButton(amount, item));
    }

    // 해당 슬롯을 비우는 메서드
    public void SetEmpty()
    {
        itemIcon.sprite = null;
        itemIcon.enabled = false;
        amountText.text = string.Empty;
    }
}
