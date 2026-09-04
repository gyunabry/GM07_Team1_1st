using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopExpansionUIController : MonoBehaviour
{
    [Header("시스템")]
    [SerializeField] private WorkshopExpansionManager expansionManager;
    [SerializeField] private PlacementSystem placementSystem;
    [SerializeField] private Player player;

    [Header("확장 버튼")]
    [SerializeField] private List<WorkshopExpansionButtonView> buttonViews = new();

    [Header("확장 확인")]
    [SerializeField] private GameObject actionButtonRoot;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private CurrencySystem currencySystem;

    private void Awake()
    {
        currencySystem = CurrencySystem.Instance;
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void Start()
    {
        currencySystem = CurrencySystem.Instance;

        foreach (WorkshopExpansionButtonView view in buttonViews)
        {
            if (view != null)
            {
                view.Clicked += HandleExpansionClicked;
            }
        }

        confirmButton?.onClick.AddListener(HandleConfirmClicked);
        cancelButton?.onClick.AddListener(HandleCancelClicked);

        if (expansionManager != null)
        {
            expansionManager.StateChanged += HandleStateChanged;
            expansionManager.SelectionChanged += HandleSelectionChanged;
        }

        if (placementSystem != null)
        {
            placementSystem.ModeChanged += HandlePlacementModeChanged;
        }

        if (currencySystem != null)
        {
            currencySystem.LevelUp += HandleLevelUp;
        }

        if (currencySystem != null)
        {
            currencySystem.CurrencyChanged += HandleCurrencyChanged;
            currencySystem.CurrencyChanged_Gold += HandleGoldChanged;
            currencySystem.CurrencyChanged_EXP += HandleExpChanged;
        }

        Refresh();
    }

    private void OnDisable()
    {
        foreach (WorkshopExpansionButtonView view in buttonViews)
        {
            if (view != null)
            {
                view.Clicked -= HandleExpansionClicked;
            }
        }

        confirmButton?.onClick.RemoveListener(HandleConfirmClicked);
        cancelButton?.onClick.RemoveListener(HandleCancelClicked);

        if (expansionManager != null)
        {
            expansionManager.StateChanged -= HandleStateChanged;
            expansionManager.SelectionChanged -= HandleSelectionChanged;
        }

        if (placementSystem != null)
        {
            placementSystem.ModeChanged -= HandlePlacementModeChanged;
        }

        if (currencySystem != null)
        {
            currencySystem.LevelUp -= HandleLevelUp;
        }

        if (currencySystem != null)
        {
            currencySystem.CurrencyChanged -= HandleCurrencyChanged;
            currencySystem.CurrencyChanged_Gold -= HandleGoldChanged;
            currencySystem.CurrencyChanged_EXP -= HandleExpChanged;
        }
    }

    private void HandleExpansionClicked(WorkshopExpansionDataSO data)
    {
        if (data == null || expansionManager == null)
        {
            return;
        }

        ExpansionPurchaseStatus status = expansionManager.Evaluate(data);

        if (!status.CanPurchase)
        {
            Refresh();
            return;
        }

        // 시설 배치 모드 종료
        placementSystem?.ExitCurrentMode();

        if (!expansionManager.SelectExpansion(data))
        {
            Debug.LogWarning($"{data.DisplayName}의 확장 프리뷰를 표시하지 못했습니다.");
        }
    }

    private void HandleConfirmClicked()
    {
        if (expansionManager == null
            || expansionManager.SelectedExpansion == null)
        {
            return;
        }

            expansionManager.TryPurchaseSelected();
        Refresh();
    }

    private void HandleCancelClicked()
    {
        if (expansionManager == null
            || expansionManager.SelectedExpansion == null)
        {
            return;
        }

        expansionManager?.CancelSelection();
    }

    private void HandlePlacementModeChanged(PlacementMode mode)
    {
        if (mode == PlacementMode.None ||
            expansionManager == null ||
            expansionManager.SelectedExpansion == null)
        {
            return;
        }

        expansionManager.CancelSelection();
    }

    private void HandleSelectionChanged(WorkshopExpansionDataSO data)
    {
        Refresh();
    }

    private void HandleStateChanged()
    {
        Refresh();
    }

    private void HandleCurrencyChanged(int money, int exp)
    {
        Refresh();
    }

    private void HandleGoldChanged(int money)
    {
        Refresh();
    }

    private void HandleExpChanged(int exp)
    {
        Refresh();
    }

    private void HandleLevelUp()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (expansionManager == null) return;

        WorkshopExpansionDataSO selected = expansionManager.SelectedExpansion;

        foreach (WorkshopExpansionButtonView button in buttonViews)
        {
            if (button == null || button.Data == null)
            {
                continue;
            }

            ExpansionPurchaseStatus status = expansionManager.Evaluate(button.Data);

            button.Refresh(status, isSelected: button.Data == selected);

            RefreshActionButtons(selected);
        }
    }

    // 판매와 같은 버튼을 공유하므로 
    private void RefreshActionButtons(WorkshopExpansionDataSO selected)
    {
        bool hasSelection = selected != null;

        if (actionButtonRoot != null)
        {
            actionButtonRoot.SetActive(hasSelection);
        }

        if (!hasSelection) return;

        ExpansionPurchaseStatus status = expansionManager.Evaluate(selected);

        if (confirmButton != null)
        {
            confirmButton.interactable = status.CanPurchase;
        }
    }
}
