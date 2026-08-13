using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInteractionPanel : MonoBehaviour
{
    [Header("캔버스")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private GraphicRaycaster graphicRaycaster;

    [Header("뷰")]
    [SerializeField] private Button itemButton;
    [SerializeField] private TMP_Text buildingNameText;

    [Header("레시피 선택 패널")]
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

        SetVisible(false);
    }

    private void OnDestroy()
    {
        if (itemButton != null)
        {
            itemButton.onClick.RemoveListener(OpenRecipePanel);
        }

        SetCurrentBuilding(null);
    }

    public void ShowPanel(IBuildingUIModel building)
    {
        if (building == null) return;

        // 현재 건물 정보를 초기화
        recipeSelectPanel.Hide();

        SetCurrentBuilding(null);

        buildingNameText.text = building.BuildingName;

        Component buildingComponent = building as Component;
        ProductionBuilding productionBuilding = null;

        // buildingComponent가 null이 아니면 currentBuilding을 ProductionBuilding으로 캐스팅
        if (buildingComponent != null)
        {
            productionBuilding = buildingComponent.GetComponent<ProductionBuilding>();
        }

        SetCurrentBuilding(productionBuilding);

        bool isProduction = currentBuilding != null;

        // currentBuilding이 null이 아니면 아이템 버튼을 활성화하고, null이면 비활성화s
        itemButton.gameObject.SetActive(isProduction);

        if (isProduction)
        {
            // 현재 생산대의 선택된 레시피로 정보를 갱신
            RefreshRecipeInfo(currentBuilding.SelectedRecipe);
            // 현재 생산대의 상태로 진행 상황을 갱신
            RefreshProductionStatus();
        }

        SetVisible(true);
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

    // 생산 진행 상황을 갱신하는 메서드
    private void RefreshProductionStatus()
    {
        if (currentBuilding == null) return;

        RecipeDataSO activeRecipe = currentBuilding.ActiveRecipe;
        ProductionState state = currentBuilding.State;

        bool hasActiveProduction = activeRecipe != null;

        if (!hasActiveProduction) return;

        RefreshStatus(state);
    }

    private void RefreshStatus(ProductionState state)
    {
        statusText.text = state switch
        {
            ProductionState.Idle => "Waiting...",
            ProductionState.WaitingForMaterials => "Waiting for ingredient",
            ProductionState.Producing => "Producing...",
            ProductionState.WaitingForOutputSpace => "Output space insufficient",
            _ => "Unknown state"
        };
    }

    private void HandleProgressChanged(float progress)
    {
        SetProgress(progress);
    }

    // 진행률을 설정하는 메서드
    private void SetProgress(float progress)
    {
        progressFill.fillAmount = Mathf.Clamp01(progress);
    }

    private void HandleStateChanged(ProductionState state)
    {
        RefreshProductionStatus();
    }

    private void HandleProductionStarted(RecipeDataSO recipe)
    {
        RefreshProductionStatus();
    }

    private void HandleProductionCompleted(RecipeDataSO recipe)
    {
        RefreshProductionStatus();
    }

    public void HidePanel()
    {
        recipeSelectPanel.Hide();
        SetCurrentBuilding(null);

        SetVisible(false);
    }

    private void OpenRecipePanel()
    {
        if (currentBuilding == null) return;

        recipeSelectPanel.Show(currentBuilding);
    }

    private void SetVisible(bool visible)
    {
        targetCanvas.enabled = visible;
        graphicRaycaster.enabled = visible;
    }

    // 현재 건물의 레시피 변경 이벤트를 구독하거나 해제하는 메서드
    private void SetCurrentBuilding(ProductionBuilding building)
    {
        if (currentBuilding != null)
        {
            currentBuilding.RecipeChanged -= HandleRecipeChanged;
            currentBuilding.ProgressChanged -= HandleProgressChanged;
            currentBuilding.StateChanged -= HandleStateChanged;
            currentBuilding.ProductionStarted -= HandleProductionStarted;
            currentBuilding.ProductionComplete -= HandleProductionCompleted;
        }

        currentBuilding = building;

        if (currentBuilding != null)
        {
            currentBuilding.RecipeChanged += HandleRecipeChanged;
            currentBuilding.ProgressChanged += HandleProgressChanged;
            currentBuilding.StateChanged += HandleStateChanged;
            currentBuilding.ProductionStarted += HandleProductionStarted;
            currentBuilding.ProductionComplete += HandleProductionCompleted;
        }
    }

    // 레시피 변경 이벤트를 처리하는 메서드
    private void HandleRecipeChanged(RecipeDataSO recipe)
    {
        RefreshRecipeInfo(recipe);
    }
}
