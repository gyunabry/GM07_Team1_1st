using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecipeSelectPanel : MonoBehaviour
{
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private GraphicRaycaster graphicRaycaster;

    [Header("레시피 리스트")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private RecipeUnlockManager recipeUnlockManager;

    private readonly List<RecipeDataSO> visibleRecipes = new();

    private RecipeButtonView[] buttonViews;
    private ProductionBuilding currentBuilding;
    private bool isSubscribedToUnlockChanges;

    private void Awake()
    {
        buttonViews = contentRoot.GetComponentsInChildren<RecipeButtonView>(true);
       
        SetVisible(false);
    }

    private void OnEnable()
    {
        ResolveUnlockManager();
        SubscribeToUnlockChanges();
    }

    private void OnDisable()
    {
        UnsubscribeFromUnlockChanges();
    }

    public void Show(ProductionBuilding building)
    {
        if (building == null) return;

        currentBuilding = building;

        ResolveUnlockManager();
        SubscribeToUnlockChanges();
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
            
            if (recipeUnlockManager != null && recipeUnlockManager.IsUnlocked(recipe))
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

    private void ResolveUnlockManager()
    {
        if (recipeUnlockManager == null)
        {
            recipeUnlockManager = RecipeUnlockManager.Instance;
        }
    }

    private void SubscribeToUnlockChanges()
    {
        if (recipeUnlockManager != null && !isSubscribedToUnlockChanges)
        {
            recipeUnlockManager.UnlockedRecipesChanged += HandleUnlockedRecipesChanged;
            isSubscribedToUnlockChanges = true;
        }
    }

    private void UnsubscribeFromUnlockChanges()
    {
        if (recipeUnlockManager != null && isSubscribedToUnlockChanges)
        {
            recipeUnlockManager.UnlockedRecipesChanged -= HandleUnlockedRecipesChanged;
            isSubscribedToUnlockChanges = false;
        }
    }

    private void HandleUnlockedRecipesChanged()
    {
        if (currentBuilding == null)
        {
            return;
        }

        CollectUnlockedRecipes();
        RefreshButtons();
    }
}
