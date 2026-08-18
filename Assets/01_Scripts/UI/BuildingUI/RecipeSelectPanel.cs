using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeSelectPanel : MonoBehaviour
{
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private GraphicRaycaster graphicRaycaster;

    [Header("레시피 리스트")]
    [SerializeField] private Transform contentRoot;

    [Header("임시 해금 레시피 데이터")]
    [SerializeField] private List<RecipeDataSO> unlockedRecipes = new();

    private readonly List<RecipeDataSO> visibleRecipes = new();

    private RecipeButtonView[] buttonViews;
    private ProductionBuilding currentBuilding;

    private void Awake()
    {
        buttonViews = contentRoot.GetComponentsInChildren<RecipeButtonView>(true);
       
        SetVisible(false);
    }

    public void Show(ProductionBuilding building)
    {
        if (building == null) return;

        currentBuilding = building;

        CollectUnlockedRecipes();
        RefreshButtons();
        SetVisible(true);
    }

    // 해금된 레시피를 수집하는 메서드
    private void CollectUnlockedRecipes()
    {
        visibleRecipes.Clear();

        foreach (RecipeDataSO recipe in currentBuilding.AvailableRecipes)
        {
            if (recipe == null) continue;
            
            if (unlockedRecipes.Contains(recipe))
            {
                visibleRecipes.Add(recipe);
            }
        }
    }

    private void RefreshButtons()
    {
        int displayCount = Mathf.Min(visibleRecipes.Count, buttonViews.Length);

        for (int i = 0; i < buttonViews.Length; i++)
        {
            RecipeButtonView buttonView = buttonViews[i];

            if (i < displayCount)
            {
                RecipeDataSO recipe = visibleRecipes[i];
                bool isSelected = currentBuilding.SelectedRecipe == recipe;

                buttonView.gameObject.SetActive(true);
                buttonView.Bind(recipe, isSelected, HandleRecipeSelected);
            }
            else
            {
                buttonView.gameObject.SetActive(false);
            }
        }
    }

    private void HandleRecipeSelected(RecipeDataSO recipe)
    {
        if (currentBuilding == null) return;

        if (!currentBuilding.TrySetRecipe(recipe)) return;

        foreach (RecipeButtonView buttonView in buttonViews)
        {
            bool selected = buttonView.BoundRecipe == recipe;
            buttonView.SetSelected(selected);
        }
    }

    public void Hide()
    {
        SetVisible(false);
        currentBuilding = null;
    }

    private void SetVisible(bool visible)
    {
        targetCanvas.enabled = visible;
        graphicRaycaster.enabled = visible;
    }
}
