using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 판매대와 통합 전송기가 같은 아이템 뷰를 사용하도록 리팩토링 예정

public class SalesInventoryItemView : MonoBehaviour
{
    [Header("아이템 정보")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text amountText;

    private Button button;

    private ItemInventory sourceInventory;
    private StorageDecomposition storageDecomposition;
    private ItemDataSO currentItem;
    private int currentAmount;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }

        SetEmpty();
    }


    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }

    public void Bind(InventoryEntry entry, ItemInventory inventory, StorageDecomposition sd = null)
    {
        if (entry == null || entry.IsEmpty || entry.Item == null)
        {
            SetEmpty();
            return;
        }

        sourceInventory = inventory;
        storageDecomposition = sd;
        currentItem = entry.Item;
        int amount = Mathf.Max(0, entry.Amount);
        currentAmount = amount;

        if (itemIcon != null)
        {
            itemIcon.sprite = currentItem.Icon;
            itemIcon.enabled = currentItem.Icon != null;
        }

        if (amountText != null) amountText.text = AmountFormat(currentAmount);

        if (button != null) button.interactable = storageDecomposition != null;
    }

    // 해당 슬롯을 비우는 메서드
    public void SetEmpty()
    {
        sourceInventory = null;
        storageDecomposition = null;
        currentItem = null;
        currentAmount = 0;

        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }

        if (amountText != null) amountText.text = string.Empty;
        if (button != null) button.interactable = false;
    }

    private void HandleClick()
    {
        if (storageDecomposition == null || 
            sourceInventory == null || 
            currentItem == null || 
            currentAmount <= 0)
        {
            return;
        }

        storageDecomposition.OnClickDecompositionButton(sourceInventory, currentItem);
    }

    // 1,000을 넘어가면 k로 표시하는 메서드
    private static string AmountFormat(int amount)
    {
        if (amount >= 1000)
        {
            return $"{amount / 1000f:0.#}k";
        }

        return $"{amount}";
    }
}
