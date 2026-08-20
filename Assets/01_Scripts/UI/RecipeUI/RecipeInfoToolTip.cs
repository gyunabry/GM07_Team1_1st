using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RecipeInfoToolTip : MonoBehaviour
{
    [Header("툴팁")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Vector2 mouseOffset;

    [Header("레시피 정보")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text outputName;
    [SerializeField] private TMP_Text outputPrice;
    [SerializeField] private TMP_Text outputTime;
    [SerializeField] private TMP_Text outputDescription;
    [SerializeField] private Image inputIcon;
    [SerializeField] private TMP_Text inputName;

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
        if (tooltipPanel.activeInHierarchy) CalculateMousePosition();
    }

    public void ShowTooltip(RecipeDataSO recipe)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
        }

        ItemDataSO input = recipe.Input;
        ItemDataSO output = recipe.Output;

        if (itemIcon != null)
        {
            itemIcon.sprite = output.Icon;
            itemIcon.enabled = true;
        }

        if (outputName != null)
        {
            outputName.text = output.ItemName;
        }

        if (outputPrice != null)
        {
            outputPrice.text = $"{output.SellPrice:N0}";
        }

        if (outputTime != null)
        {
            outputTime.text = $"{recipe.ProductionTime} s";
        }

        if (outputDescription != null)
        {
            outputDescription.text = output.Description;
        }

        if (inputIcon != null)
        {
            inputIcon.sprite = input.Icon;
            inputIcon.enabled = true;
        }

        if (inputName != null)
        {
            inputName.text = input.ItemName;
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
