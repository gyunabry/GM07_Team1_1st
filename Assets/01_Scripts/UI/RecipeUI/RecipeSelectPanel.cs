using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeSelectPanel : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private GraphicRaycaster graphicRaycaster;

    [Header("스크롤 뷰")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform contentRoot;

    [Header("레시피 버튼 프리팹")]
    [SerializeField] private RecipeButtonView recipeButtonPrefab;
    [SerializeField] private int preSpawnCount = 3;

    [Header("레시피 툴팁")]
    [SerializeField] private RecipeInfoToolTip recipeToolTip;

    [Header("임시 해금 레시피 데이터")]
    [SerializeField] private List<RecipeDataSO> unlockedRecipes = new();

    // 실제로 해당 시설에서 보여지는 레시피
    private readonly List<RecipeDataSO> visibleRecipes = new();
    private readonly List<RecipeButtonView> buttonViews = new();

    private ProductionBuilding currentBuilding;

    private void Awake()
    {
        // 기존 버튼이 남아있따면 풀에 포함
        CacheExistingButtons();

        // 지정한 개수만큼 버튼 생성
        EnsureButtonCount(preSpawnCount);

        DeactivateButtonsFrom(0);
        SetVisible(false);
    }

    public void Show(ProductionBuilding building)
    {
        if (building == null) return;

        currentBuilding = building;

        CollectUnlockedRecipes();

        EnsureButtonCount(visibleRecipes.Count);

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

            // 해당 시설에서 해당 레시피를 사용할 수 없으면 패스
            if (!currentBuilding.CanProcess(recipe)) continue;
            if (!unlockedRecipes.Contains(recipe)) continue;

            visibleRecipes.Add(recipe);
        }
    }

    // 기존 버튼을 저장
    private void CacheExistingButtons()
    {
        RecipeButtonView[] existingButtons = contentRoot.GetComponentsInChildren<RecipeButtonView>(true);

        foreach (RecipeButtonView button in existingButtons)
        {
            if (button == null || buttonViews.Contains(button)) continue;

            ConfigureButton(button);

            button.Unbind();
            button.gameObject.SetActive(false);

            buttonViews.Add(button);
        }
    }

    private void EnsureButtonCount(int requiredCount)
    {
        if (recipeButtonPrefab == null || contentRoot == null)
        {
            return;
        }

        while (buttonViews.Count < requiredCount)
        {
            RecipeButtonView buttonView = Instantiate(recipeButtonPrefab, contentRoot, false);

            buttonView.name = $"Recipe_Button_{buttonViews.Count:00}";

            ConfigureButton(buttonView);

            buttonView.Unbind();
            buttonView.gameObject.SetActive(false);

            buttonViews.Add(buttonView);
        }
    }

    private void ConfigureButton(RecipeButtonView buttonView)
    {
        buttonView.SetTooltip(recipeToolTip);
    }

    private void RefreshButtons()
    {
        recipeToolTip?.HideTooltip();

        for (int i = 0; i < buttonViews.Count; i++)
        {
            RecipeButtonView buttonView = buttonViews[i];

            if (i >= visibleRecipes.Count)
            {
                buttonView.Unbind();
                buttonView.gameObject.SetActive(false);
                continue;
            }

            RecipeDataSO recipe = visibleRecipes[i];

            bool isSelected = currentBuilding.SelectedRecipe == recipe;

            buttonView.Bind(recipe, isSelected, HandleRecipeSelected);
            buttonView.gameObject.SetActive(true);
        }
    }

    private void HandleRecipeSelected(RecipeDataSO recipe)
    {
        if (currentBuilding == null || recipe == null) return;

        if (!currentBuilding.TrySetRecipe(recipe)) return;

        foreach (RecipeButtonView buttonView in buttonViews)
        {
            if (!buttonView.gameObject.activeSelf) continue;

            bool selected = buttonView.BoundRecipe == recipe;
            buttonView.SetSelected(selected);
        }
    }

    private void DeactivateButtonsFrom(int startIndex)
    {
        for (int i = startIndex; i < buttonViews.Count; i++)
        {
            buttonViews[i].Unbind();
            buttonViews[i].gameObject.SetActive(false);
        }
    }

    public void Hide()
    {
        recipeToolTip?.HideTooltip();

        SetVisible(false);
        currentBuilding = null;
    }

    private void SetVisible(bool visible)
    {
        targetCanvas.enabled = visible;
        graphicRaycaster.enabled = visible;
        if (!visible)
        {
            recipeToolTip?.HideTooltip();
        }
    }
}
