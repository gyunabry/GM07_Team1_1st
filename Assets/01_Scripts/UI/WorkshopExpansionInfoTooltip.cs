using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WorkshopExpansionInfoTooltip : MonoBehaviour
{
    [Header("Tooltip")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Vector2 mouseOffset;

    [Header("Expansion Information")]
    [SerializeField] private Image expansionIcon;
    [SerializeField] private TMP_Text expansionName;
    [SerializeField] private TMP_Text expansionPrice;
    [SerializeField] private TMP_Text requiredLevel;
    [SerializeField] private TMP_Text expansionDescription;

    private RectTransform tooltipRect;
    private RectTransform tooltipParentRect;

    private void Awake()
    {
        if (tooltipPanel == null) return;

        tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        tooltipParentRect = tooltipRect != null
            ? tooltipRect.parent as RectTransform
            : null;
    }

    private void Start()
    {
        HideTooltip();
    }

    private void OnDisable()
    {
        HideTooltip();
    }

    private void LateUpdate()
    {
        if (tooltipPanel != null && tooltipPanel.activeInHierarchy)
        {
            CalculateMousePosition();
        }
    }

    public void ShowTooltip(WorkshopExpansionDataSO data)
    {
        if (data == null || tooltipPanel == null)
        {
            HideTooltip();
            return;
        }

        if (expansionIcon != null)
        {
            expansionIcon.sprite = data.Icon;
            expansionIcon.enabled = data.Icon != null;
        }

        if (expansionName != null)
        {
            expansionName.text = data.DisplayName;
        }

        if (expansionPrice != null)
        {
            expansionPrice.text = $"{data.Price:N0}G";
        }

        if (requiredLevel != null)
        {
            requiredLevel.text = data.RequiredLevel > 0
                ? $"Lv. {data.RequiredLevel}"
                : "조건 없음";
        }

        if (expansionDescription != null)
        {
            expansionDescription.text = data.Description;
        }

        tooltipPanel.SetActive(true);

        if (tooltipRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
        }

        CalculateMousePosition();
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
        if (tooltipRect == null
            || tooltipParentRect == null
            || canvas == null
            || Mouse.current == null)
        {
            return;
        }

        Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                tooltipParentRect,
                Mouse.current.position.ReadValue(),
                eventCamera,
                out Vector2 localPosition))
        {
            return;
        }

        Vector3 targetLocalPosition = tooltipRect.localPosition;
        targetLocalPosition.x = localPosition.x + mouseOffset.x;
        targetLocalPosition.y = localPosition.y + mouseOffset.y;
        tooltipRect.localPosition = targetLocalPosition;

        ClampToParentBounds();
    }

    private void ClampToParentBounds()
    {
        Rect parentRect = tooltipParentRect.rect;
        Bounds tooltipBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
            tooltipParentRect,
            tooltipRect);

        float minCenterX = parentRect.xMin + tooltipBounds.extents.x;
        float maxCenterX = parentRect.xMax - tooltipBounds.extents.x;
        float minCenterY = parentRect.yMin + tooltipBounds.extents.y;
        float maxCenterY = parentRect.yMax - tooltipBounds.extents.y;

        float clampedCenterX = minCenterX <= maxCenterX
            ? Mathf.Clamp(tooltipBounds.center.x, minCenterX, maxCenterX)
            : parentRect.center.x;
        float clampedCenterY = minCenterY <= maxCenterY
            ? Mathf.Clamp(tooltipBounds.center.y, minCenterY, maxCenterY)
            : parentRect.center.y;

        Vector3 correction = new(
            clampedCenterX - tooltipBounds.center.x,
            clampedCenterY - tooltipBounds.center.y,
            0f);

        tooltipRect.localPosition += correction;
    }
}
