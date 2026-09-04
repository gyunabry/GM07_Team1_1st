using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorkshopExpansionButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("확장 데이터")]
    [SerializeField] private WorkshopExpansionDataSO expansionData;

    [Header("Tooltip")]
    [SerializeField] private WorkshopExpansionInfoTooltip tooltip;

    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private GameObject unavailableImage;
    [SerializeField] private GameObject checkImage;         // 구매 완료된 확장 버튼에 표시할 체크 이미지

    [Header("색상")]
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color unavailableColor = Color.red;

    public WorkshopExpansionDataSO Data => expansionData;

    public event Action<WorkshopExpansionDataSO> Clicked;

    private void Awake()
    {
        if (button != null)
        {
            button.onClick.AddListener(HandleClicked);
        }

        BindData();
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClicked);
        }
    }

    private void OnDisable()
    {
        tooltip?.HideTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip?.ShowTooltip(expansionData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip?.HideTooltip();
    }

    private void HandleClicked()
    {
        if (expansionData == null) return;

        Clicked?.Invoke(expansionData);
    }

    private void BindData()
    {
        if (expansionData == null)
        {
            if (button != null)
            {
                button.interactable = false;
            }

            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = expansionData.Icon;
            iconImage.enabled = true;
        }

        if (nameText != null)
        {
            nameText.text = expansionData.DisplayName;
        }

        if (priceText != null)
        {
            priceText.text = $"{expansionData.Price:N0}G";
        }
    }

    public void Refresh(ExpansionPurchaseStatus status, bool isSelected)
    {
        if (expansionData == null) return;

        if (nameText != null)
        {
            nameText.color = status.CanPurchase
                ? availableColor
                : unavailableColor;
        }

        if (priceText != null)
        {
            priceText.text = $"{status.Price:N0}G";
        }

        if (button != null)
        {
            button.interactable = status.CanPurchase;
        }

        if (unavailableImage != null)
        {
            unavailableImage.SetActive(!status.CanPurchase);
        }

        if (checkImage != null)
        {
            checkImage.SetActive(status.HasReason(ExpansionBlockReason.AlreadyPurchase));
        }
    }
} 
