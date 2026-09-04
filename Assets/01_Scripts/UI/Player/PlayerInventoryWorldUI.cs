using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventoryWorldUI : MonoBehaviour
{
    [Header("HUD 설정")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Image InventoryIcon;

    [Header("글자 색상")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color maxColor = Color.red;

    private ItemInventory inventory;

    private Vector3 originTextScale;
    private Vector3 originIconScale;

    private void Awake()
    {
        PlayerInventory playerInventory = GetComponentInParent<PlayerInventory>();

        if (playerInventory != null) inventory = playerInventory.Inventory;

        originTextScale = countText.rectTransform.localScale;
        originIconScale = InventoryIcon.rectTransform.localScale;
    }

    private void OnEnable()
    {
        if (inventory == null) return;

        inventory.InventoryChanged += RefreshUI;
        inventory.OnInventoryChanged += InventoryEffect;
        RefreshUI();
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.InventoryChanged -= RefreshUI;
            inventory.OnInventoryChanged -= InventoryEffect;
        }
    }

    private void RefreshUI()
    {
        if (inventory == null) return;

        int totalAmount = inventory.TotalAmount;
        SetVisible(totalAmount > 0);

        if (totalAmount <= 0 || countText == null) return;

        if (inventory.RemainingCapacity == 0)
        {
            countText.text = "MAX";
            countText.color = maxColor;
            countText.fontSize = 0.4f;
        }
        else
        {
            countText.text = $"{inventory.TotalAmount} / {inventory.Capacity}";
            countText.color = defaultColor;
            countText.fontSize = 0.325f;
        }  
    }

    private void SetVisible(bool visible)
    {
        if (canvas != null)
        {
            canvas.enabled = visible;
        }
    }

    // DOTween 추가
    private void InventoryEffect()
    {
        countText.transform.DOKill();
        InventoryIcon.transform.DOKill();

        countText.transform.DOScale(1.4f, 0.15f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                countText.transform.DOScale(originTextScale, 0.12f).SetEase(Ease.OutQuad);
            });

        InventoryIcon.transform.DOScale(1.4f, 0.15f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                InventoryIcon.transform.DOScale(originIconScale, 0.12f).SetEase(Ease.OutQuad);
            });
    }
}
