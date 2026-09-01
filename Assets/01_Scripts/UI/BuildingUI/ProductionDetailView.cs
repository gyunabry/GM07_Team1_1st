using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductionDetailView : BuildingDetailView
{
    [Header("패널")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private GraphicRaycaster graphicRaycaster;
    [SerializeField] private TMP_Text buildingNameText;

    [Header("생산 아이템")]
    [SerializeField] private Button itemButton; // 완성품 아이콘 버튼
    [SerializeField] private Image inputIcon;
    [SerializeField] private TMP_Text inputCountText;
    [SerializeField] private Image addIcon; // 레시피 선택 전에 보여줄 + 이미지
    [SerializeField] private Image outputIcon;
    [SerializeField] private TMP_Text outputCountText;
    [SerializeField] private TMP_Text productionTimeText;
    [SerializeField] private Image progressFill;

    [Header("레시피")]
    [SerializeField] private RecipeSelectPanel recipeSelectPanel;

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
    }

    public void Show(ProductionBuilding building, string buildingName)
    {
        if (building == null) return;

        Hide();

        currentBuilding = building;

        if (buildingNameText != null)
        {
            buildingNameText.text = buildingName;
        }

        if (currentBuilding != null)
        {
            currentBuilding.RecipeChanged += HandleRecipeChanged;
            currentBuilding.ProgressChanged += HandleProgressChanged;
            currentBuilding.StateChanged += HandleStateChanged;

            currentBuilding.InputInventory.InventoryChanged += HandleInventoryChanged;
            currentBuilding.OutputInventory.InventoryChanged += HandleInventoryChanged;
        }

        RefreshAll();
        SetVisible(true);
    }

    public void Hide()
    {
        recipeSelectPanel?.Hide();

        DetachBuilding();
        ResetView();
        SetVisible(false);
    }

    private void DetachBuilding()
    {
        if (currentBuilding == null) return;

        currentBuilding.RecipeChanged -= HandleRecipeChanged;
        currentBuilding.ProgressChanged -= HandleProgressChanged;
        currentBuilding.StateChanged -= HandleStateChanged;

        currentBuilding.InputInventory.InventoryChanged -= HandleInventoryChanged;
        currentBuilding.OutputInventory.InventoryChanged -= HandleInventoryChanged;

        currentBuilding = null;
    }

    private void RefreshAll()
    {
        if (currentBuilding == null)
        {
            ResetView();
            return;
        }

        RefreshRecipeInfo(currentBuilding.SelectedRecipe);
        SetProgress(currentBuilding.Progress);
        RefreshRemainingTime();
        RefreshInventoryInfo();
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

    // 레시피가 선택됐을 때 호출되는 메서드
    private void RefreshRecipeInfo(RecipeDataSO recipe)
    {
        if (recipe == null)
        {
            ResetView();
            return;
        }

        if (addIcon != null)
        {
            addIcon.enabled = false;
        }

        if (inputIcon != null)
        {
            inputIcon.sprite = recipe.Input.Icon;
            inputIcon.enabled = true;
        }

        if (outputIcon != null)
        {
            outputIcon.sprite = recipe.Output.Icon;
            outputIcon.enabled = true;
        }

        RefreshInventoryInfo();
    }

    // 생산시설 인벤토리 갱신
    private void RefreshInventoryInfo()
    {
        if (currentBuilding == null) return;

        RecipeDataSO recipe = currentBuilding.SelectedRecipe;

        // 레시피가 선택되지 않은 상태일때
        if (recipe == null)
        {
            inputCountText.text = string.Empty;
            outputCountText.text = string.Empty;
            addIcon.enabled = true;
            return;
        }

        int inputAmount = currentBuilding.InputInventory.GetAmount(recipe.Input);
        int outputAmount = currentBuilding.OutputInventory.GetAmount(recipe.Output);

        if (inputCountText != null)
        {
            inputCountText.text = $"{inputAmount} / {currentBuilding.InputInventory.Capacity}";
        }

        if (outputCountText != null)
        {
            outputCountText.text = $"{outputAmount} / {currentBuilding.OutputInventory.Capacity}";
        }
    }

    private void RefreshRemainingTime()
    {
        if (currentBuilding == null || currentBuilding.SelectedRecipe == null)
        {
            productionTimeText.text = "00:00";
            return;
        }

        float time;

        if (currentBuilding.ActiveRecipe != null)
        {
            // 생산 중 or 출력 공간 부족
            time = currentBuilding.ReaminingTime;
        }
        else
        {
            // 재료 대기 or 생산 시작 전
            time = currentBuilding.SelectedRecipeEffectiveDuration;
        }

        int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, time));

        int min = totalSeconds / 60;
        int sec = totalSeconds % 60;

        if (productionTimeText != null)
        {
            productionTimeText.text = $"{min:00}:{sec:00}";
        }
    }

    private void ResetView()
    {
        if (inputIcon != null)
        {
            inputIcon.sprite = null;
            inputIcon.enabled = false;
        }

        if (outputIcon != null)
        {
            outputIcon.sprite = null;
            outputIcon.enabled = false;
        }

        if (productionTimeText != null) productionTimeText.text = string.Empty;
        if (inputCountText != null) inputCountText.text = string.Empty;
        if (outputCountText != null) outputCountText.text = string.Empty;

        SetProgress(0f);
    }

    private void HandleRecipeChanged(RecipeDataSO recipe)
    {
        RefreshRecipeInfo(recipe);
        RefreshRemainingTime();
    }

    private void HandleProgressChanged(float progress)
    {
        SetProgress(progress);
        RefreshRemainingTime();
    }

    private void HandleStateChanged(ProductionState state)
    {
        if (currentBuilding != null)
        {
            SetProgress(currentBuilding.Progress);
        }
    }

    private void HandleInventoryChanged()
    {
        RefreshInventoryInfo();
    }

    private void SetVisible(bool visible)
    {
        targetCanvas.enabled = visible;
        graphicRaycaster.enabled = visible;
    }

    public override bool Supports(IBuildingUIModel building)
    {
        return GetBuildingComponent<ProductionBuilding>(building) != null;
    }

    public override void Open(IBuildingUIModel building)
    {
        ProductionBuilding productionBuilding = GetBuildingComponent<ProductionBuilding>(building);

        if (productionBuilding == null) return;

        Show(productionBuilding, building.BuildingName);
    }

    public override void Close()
    {
        Hide();
    }
}