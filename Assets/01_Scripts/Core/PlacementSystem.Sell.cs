using UnityEngine;

public partial class PlacementSystem : MonoBehaviour
{
    [SerializeField] private BuildingSaleService buildingSaleService;

    private bool salePorcessing;

    public void BeginSellMode()
    {
        if (CurrentMode != PlacementMode.None) return;

        ClearSelection();
        ChangeMode(PlacementMode.SellSelect);
    }

    private void TrySelectSellTargetPointer()
    {
        if (!inputManager.TryGetBuilding(out PlacedBuilding building))
        {
            return;
        }

        TrySelectSellTarget(building);
    }

    public bool TrySelectSellTarget(PlacedBuilding building)
    {
        if (!IsSellMode || building == null) return false;

        BuildingSaleEvaluation evaluation = buildingSaleService.Evaluate(building);

        if (!evaluation.CanSell)
        {
            return false;
        }

        ClearSelection();

        selectedPlacedBuilding = building;

        // 판매 가능한 건물 선택 및 판매 확인 모드로 진입
        SelectionChanged?.Invoke(building);
        ChangeMode(PlacementMode.SellConfirm);

        return true;
    }

    public void ConfirmSell()
    {
        if (CurrentMode != PlacementMode.SellConfirm ||
            selectedPlacedBuilding == null ||
            salePorcessing)
        {
            return;
        }

        salePorcessing = true;

        PlacedBuilding building = selectedPlacedBuilding;

        BuildingSaleEvaluation evaluation = buildingSaleService.Evaluate(building);

        if (!evaluation.CanSell)
        {
            ClearSelection();
            ChangeMode(PlacementMode.SellSelect);
            return;
        }

        CurrencySystem currencySystem = CurrencySystem.Instance;

        if (currencySystem == null)
        {
            ClearSelection();
            ChangeMode(PlacementMode.SellSelect);
            return;
        }

        // 가능한 재고 이관
        buildingSaleService.TransferInventoryForSale(building);

        // 점유 셀 해제
        building.AssignedArea?.Release(building, building.OccupiedCells);

        currencySystem.GrantMoney(evaluation.Refund);

        OnBuildingSold(building, evaluation.Refund);

        ClearSelection();

        Destroy(building.gameObject);

        ChangeMode(PlacementMode.SellSelect);

        salePorcessing = false;
    }

    private void ClearSelection()
    {
        selectedPlacedBuilding = null;
        SelectionChanged?.Invoke(null);
    }

    public void ToggleSellMode()
    {
        if (IsSellMode)
        {
            AudioManager.Instance.PlaySFX(ESFXType.UI_Close);
            ExitCurrentMode();
            return;
        }

        if (CurrentMode != PlacementMode.None)
        {
            ExitCurrentMode();
        }

        AudioManager.Instance.PlaySFX(ESFXType.UI_Open);
        BeginSellMode();
    }
}
