using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductionDetailView : BuildingDetailView
{
    [Header("레시피 선택 패널")]
    [SerializeField] private Button itemButton;
    [SerializeField] private RecipeSelectPanel recipeSelectPanel;

    [Header("선택된 레시피 정보")]
    [SerializeField] private Image recipeIcon;
    [SerializeField] private TMP_Text recipeNameText;
    [SerializeField] private TMP_Text itemIncomeText;
    [SerializeField] private TMP_Text productionTimeText;

    [Header("생산 진행 상황")]
    [SerializeField] private Image progressFill;
    [SerializeField] private TMP_Text statusText;

    private ProductionBuilding currentBuilding;

    private void Awake()
    {
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(OpenRecipePanel);
        }
    }

    private void OnDisable()
    {
        Unbind();
    }

    private void OnDestroy()
    {
        if (itemButton != null)
        {
            itemButton.onClick.RemoveListener(OpenRecipePanel);
        }

        Unbind();
    }

    public override void Bind(IBuildingUIModel building)
    {
        ProductionBuilding productionBuilding = GetBuildingComponent<ProductionBuilding>(building);

        Unbind();

        if (productionBuilding == null) return;

        currentBuilding = productionBuilding;

        // 시설 상태를 표시하는 동안만 이벤트 구독
        currentBuilding.RecipeChanged += HandleRecipeChanged;
        currentBuilding.ProgressChanged += HandleProgressChanged;
        currentBuilding.StateChanged += HandleStateChanged;
        currentBuilding.ProductionStarted += HandleProductionStarted;
        currentBuilding.ProductionComplete += HandleProductionCompleted;

        recipeSelectPanel?.Hide();
        RefreshAll();
    }

    public override bool Supports(IBuildingUIModel building)
    {
        return GetBuildingComponent<ProductionBuilding>(building);
    }

    public override void Unbind()
    {
        recipeSelectPanel?.Hide();

        if (currentBuilding != null)
        {
            currentBuilding.RecipeChanged -= HandleRecipeChanged;
            currentBuilding.ProgressChanged -= HandleProgressChanged;
            currentBuilding.StateChanged -= HandleStateChanged;
            currentBuilding.ProductionStarted -= HandleProductionStarted;
            currentBuilding.ProductionComplete -= HandleProductionCompleted;
        }

        currentBuilding = null;
        ResetView();
    }

    private void RefreshAll()
    {
        if (currentBuilding == null)
        {
            ResetView();
            return;
        }

        RefreshRecipeInfo(currentBuilding.SelectedRecipe);
        RefreshStatus(currentBuilding.State);
        SetProgress(currentBuilding.Progress);
    }

    // 레시피 정보를 갱신하는 메서드
    private void RefreshRecipeInfo(RecipeDataSO recipe)
    {
        if (recipe == null)
        {
            recipeIcon.enabled = false;
            recipeNameText.text = "선택된 레시피 없음";
            itemIncomeText.text = string.Empty;
            productionTimeText.text = string.Empty;
            return;
        }

        recipeNameText.text = recipe.RecipeName;
        productionTimeText.text = $"{recipe.ProductionTime} sec";

        if (recipe.Output != null)
        {
            recipeIcon.enabled = true;
            recipeIcon.sprite = recipe.Output.Icon;
            itemIncomeText.text = $"{recipe.Output.SellPrice}G";
        }
        else
        {
            recipeIcon.enabled = false;
            itemIncomeText.text = string.Empty;
        }
    }

    private void RefreshStatus(ProductionState state)
    {
        statusText.text = state switch
        {
            ProductionState.Idle => "레시피를 선택하세요.",
            ProductionState.WaitingForMaterials => "재료 대기 중",
            ProductionState.Producing => "생산 중...",
            ProductionState.WaitingForOutputSpace => "출력 공간 부족",
            _ => "알 수 없는 상태"
        };
    }

    // 진행률을 설정하는 메서드
    private void SetProgress(float progress)
    {
        progressFill.fillAmount = Mathf.Clamp01(progress);
    }

    private void OpenRecipePanel()
    {
        if (currentBuilding == null) return;

        recipeSelectPanel.Show(currentBuilding);
    }

    private void ResetView()
    {
        if (recipeIcon != null)
        {
            recipeIcon.sprite = null;
            recipeIcon.enabled = false;
        }

        if (recipeNameText != null) recipeNameText.text = "선택된 레시피 없음";
        if (itemIncomeText != null) itemIncomeText.text = string.Empty;
        if (productionTimeText != null) productionTimeText.text = string.Empty;
        if (statusText != null) statusText.text = string.Empty;
        SetProgress(0f);
    }

    private void HandleRecipeChanged(RecipeDataSO recipe)
    {
        RefreshRecipeInfo(recipe);
        RefreshStatus(currentBuilding.State);
    }

    private void HandleProgressChanged(float progress)
    {
        SetProgress(progress);
    }

    private void HandleStateChanged(ProductionState state)
    {
        RefreshStatus(state);

        if (currentBuilding != null)
        {
            SetProgress(currentBuilding.Progress);
        }
    }

    private void HandleProductionStarted(RecipeDataSO recipe)
    {
        RefreshAll();
    }

    private void HandleProductionCompleted(RecipeDataSO recipe)
    {
        RefreshAll();
    }
}
