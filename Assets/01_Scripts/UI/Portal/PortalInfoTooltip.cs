using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PortalInfoTooltip : MonoBehaviour
{
    [Header("툴팁")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private Canvas canvas; // 부모 캔버스
    [SerializeField] private Vector2 mouseOffset;

    [Header("사냥터 정보")]
    [SerializeField] private Image fieldIcon;
    [SerializeField] private TMP_Text displayNameText;
    [SerializeField] private TMP_Text requiredLevelText;
    [SerializeField] private TMP_Text unlockCostText;

    [Header("텍스트 컬러")]
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color unavailbaleColor = Color.red;

    private RectTransform tooltipRect;
    private RectTransform tooltipParentRect;

    private void Awake()
    {
        tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        tooltipParentRect = tooltipRect.parent as RectTransform;
    }

    private void Start()
    {
        HideTooltip();
    }

    private void LateUpdate()
    {
        // 활성화된 동안 마우스 위치 추적
        if (tooltipPanel.activeInHierarchy) CalculateMousePosition();
    }

    public void ShowTooltip(HuntingFieldUnlockDataSO data, bool isUnlocked)
    {
        if (tooltipPanel != null)
        {
            HideTooltip();
            return;
        }

        CurrencySystem currency = CurrencySystem.Instance;

        bool levelSatisfied = currency != null && currency.Level >= data.RequiredLevel;
        bool moneySatisfied = currency != null && currency.Money >= data.UnlockCost;

        if (fieldIcon != null)
        {
            fieldIcon.sprite = data.FieldIcon;
            fieldIcon.enabled = true;
        }

        if (displayNameText != null)
        {
            displayNameText.text = data.DisplayName;
            displayNameText.color = isUnlocked ? availableColor : unavailbaleColor;
        }

        if (requiredLevelText != null)
        {
            requiredLevelText.text = $"요구 레벨 : Lv. {data.RequiredLevel}";
            requiredLevelText.color = levelSatisfied ? availableColor : unavailbaleColor;
        }

        if (unlockCostText != null)
        {
            unlockCostText.text = $"해금 비용 : {data.UnlockCost:N0}G";
            unlockCostText.color = moneySatisfied ? availableColor : unavailbaleColor;
        }
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    private void CalculateMousePosition()
    {
        Vector2 localPosition; // 변환된 canvas내 현재 좌표
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // 마우스 좌표를 canvas 내에서의 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tooltipParentRect,
            mousePosition,
            canvas.worldCamera,
            out localPosition
        );

        // 위치 변경
        tooltipRect.anchoredPosition = localPosition + mouseOffset;
    }
}
