using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class WorkshopExpansionManager : MonoBehaviour
{
    [Header("확장 데이터")]
    [SerializeField] private List<WorkshopExpansionDataSO> expansions = new();

    [Header("확장 규칙")]
    [Tooltip("한 번 확장으로 늘어날 크기")]
    [SerializeField] private int expansionUnit = 4;

    [Header("시스템")]
    [SerializeField] private Player player;
    [SerializeField] private BuildableArea workshopArea;
    [SerializeField] private NavMeshSurface navMeshSurface;

    [Header("연출")]
    [SerializeField] private WorkshopExpansionPreview preview;
    [SerializeField] private WorkshopStageVisualController visualController;

    [Header("저장")]
    [SerializeField] private WorkshopExpansionSaveService saveService;

    private RectInt baseBounds;
    private int currentStage;
    private bool isPurchasing;

    private WorkshopExpansionDataSO selectedExpansion;

    public int CurrentStage => currentStage;

    public WorkshopExpansionDataSO SelectedExpansion => selectedExpansion;

    public event Action StateChanged;
    public event Action<WorkshopExpansionDataSO> SelectionChanged;

    private void Awake()
    {
        if (workshopArea == null) return;

        baseBounds = workshopArea.UnlockedAreas[0];

        // currentStage = saveService != null ? saveService.LoadStage() : 0;

        currentStage = 0;

        // RestoreStage(currentStage);
    }

    // 목표 확장 영역 계산
    public RectInt GetBountForStage(int stage)
    {
        stage = Mathf.Max(0, stage);

        // 기본 크기에서 추가될 값
        int delta = expansionUnit * stage;

        // 기본 : -8, -8, 20, 20
        return new RectInt(
            baseBounds.x - delta,
            baseBounds.y - delta,
            baseBounds.width + delta,
            baseBounds.height + delta);
    }

    public ExpansionPurchaseStatus Evaluate(WorkshopExpansionDataSO data)
    {
        if (data == null) return default;

        ExpansionBlockReason reasons = ExpansionBlockReason.None;

        // 해당 데이터의 인덱스보다 현재 단계가 높다면 구매한 확장으로 표시
        if (data.StageIndex <= currentStage)
        {
            reasons |= ExpansionBlockReason.AlreadyPurchase;
        }

        // 해당 확장 데이터의 인덱스가 현재 단계보다 한 단계 높다면 이전 확장 필요
        if (data.StageIndex > currentStage + 1)
        {
            
            reasons |= ExpansionBlockReason.PreviousExpansionRequired;
        }

        // TODO: 추후 레벨 구조 변경 시 수정 필요
        if (player == null || player.NowLevel < data.RequiredLevel)
        {
            reasons |= ExpansionBlockReason.LevelRequired;
        }

        if (CurrencySystem.Instance == null || CurrencySystem.Instance.Money < data.Price)
        {
            reasons |= ExpansionBlockReason.NotEnoughMoney;
        }

        return new ExpansionPurchaseStatus(data.Price, reasons);
    }

    public bool SelectExpansion(WorkshopExpansionDataSO data)
    {
        if (data == null) return false;

        ExpansionPurchaseStatus status = Evaluate(data);

        if (!status.CanPurchase) return false;

        if (preview == null || !preview.Show(data.StageIndex))
        {
            return false;
        }

        selectedExpansion = data;

        SelectionChanged?.Invoke(data);
        StateChanged?.Invoke();

        return true;
    }

    public bool TryPurchaseSelected()
    {
        if (isPurchasing || selectedExpansion == null)
        {
            return false;
        }

        WorkshopExpansionDataSO data = selectedExpansion;
        ExpansionPurchaseStatus status = Evaluate(data);

        if (!status.CanPurchase)
        {
            StateChanged?.Invoke();
            return false;
        }

        RectInt targetBounds = GetBountForStage(data.StageIndex);
        RectInt[] areasToUnlock = { targetBounds };

        isPurchasing = true;

        if (!CurrencySystem.Instance.TrySpendMoney(data.Price))
        {
            isPurchasing = false;
            StateChanged?.Invoke();
            return false;
        }

        preview?.Hide();

        workshopArea.UnlockAreas(areasToUnlock);

        currentStage = data.StageIndex;

        visualController?.ApplyStage(currentStage);

        selectedExpansion = null;

        // saveService?.SaveStage(currentStage);

        SelectionChanged?.Invoke(null);
        StateChanged?.Invoke();

        if (navMeshSurface != null)
        {
            StartCoroutine(RebuildNavMesh());
        }

        isPurchasing = false;
        return true;
    }

    public void CancelSelection()
    {
        preview?.Hide();

        if (selectedExpansion == null) return;

        selectedExpansion = null;

        SelectionChanged?.Invoke(null);
        StateChanged?.Invoke();
    }

    private void RestoreStage(int stage)
    {
        stage = Mathf.Clamp(stage, 0, expansions.Count);

        RectInt targetBounds = GetBountForStage(stage);

        currentStage = stage;
        visualController?.ApplyStage(currentStage);

        if (stage > 0 && navMeshSurface != null)
        {
            StartCoroutine(RebuildNavMesh());
        }
    }


    private IEnumerator RebuildNavMesh()
    {
        yield return null;
       
        navMeshSurface.BuildNavMesh();
    }
}
