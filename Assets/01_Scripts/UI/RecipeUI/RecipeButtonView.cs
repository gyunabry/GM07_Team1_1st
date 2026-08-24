using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 아이템 레시피를 보여주는 버튼 뷰
/// </summary>
public class RecipeButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("툴팁")]
    [SerializeField] private RecipeInfoToolTip recipeTooltip;

    [Header("버튼")]
    [SerializeField] private Button selectButton;
    [SerializeField] private Image itemIcon;
    [SerializeField] private GameObject selectedMarker;

    private RecipeDataSO boundRecipe;

    private Action<RecipeDataSO> onClicked;

    public RecipeDataSO BoundRecipe => boundRecipe;

    private void Awake()
    {
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(HandleClicked);
        }
    }

    private void OnDestroy()
    {
        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(HandleClicked);
        }
    }

    public void Bind(RecipeDataSO recipe, bool isSelected, Action<RecipeDataSO> clickCallback) 
    {
        boundRecipe = recipe;
        onClicked = clickCallback;

        ItemDataSO output = recipe != null ? recipe.Output : null;

        if (itemIcon != null)
        {
            itemIcon.sprite = output != null ? output.Icon : null;
            itemIcon.enabled = output != null && output.Icon != null;
        }

        SetSelected(isSelected);

        if (selectButton != null)
        {
            selectButton.interactable = recipe != null;
        }
    }

    public void Unbind()
    {
        boundRecipe = null;

        onClicked = null;

        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }

        SetSelected(false);
    }

    // 의존성 주입
    public void SetTooltip(RecipeInfoToolTip tooltip)
    {
        recipeTooltip = tooltip;
    }

    private void HandleClicked()
    {
        // 버튼 클릭시 처리할 로직
        if (boundRecipe == null) return;

        onClicked?.Invoke(boundRecipe);
    }

    public void SetSelected(bool selected)
    {
        selectedMarker.SetActive(selected);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        recipeTooltip.ShowTooltip(boundRecipe);    
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        recipeTooltip.HideTooltip();
    }
}
