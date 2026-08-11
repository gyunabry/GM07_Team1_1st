using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 아이템 레시피를 보여주는 버튼 뷰
/// </summary>
public class RecipeButtonView : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemIncomeText;
    [SerializeField] private TMP_Text productionTimeText; 
    [SerializeField] private Image ingredientIcon;
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

    private void OnDisable()
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

        itemIcon.sprite = recipe.Output.Icon;

        itemNameText.text = recipe.RecipeName;
        itemIncomeText.text = $"{recipe.Output.SellPrice} G";
        productionTimeText.text = $"{recipe.ProductionTime} sec";

        selectedMarker.SetActive(isSelected);
        selectButton.interactable = true;
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
}
