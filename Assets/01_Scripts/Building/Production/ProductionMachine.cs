using System;
using UnityEngine;

// 생산 상태 
public enum ProductionState
{
    Disable,                // 건설 중 or 비활성 상태
    Idle,                   // 레시피 미선택
    WaitingForMaterials,    // 재료 부족
    Producing,              // 생산 중
    WaitingForOutputSpace   // 생산 시간 지났으나 결과물 공간 부족
}

public class ProductionMachine
{
    private readonly ItemInventory inputInventory;
    private readonly ItemInventory outputInventory;

    private RecipeDataSO selectedRecipe;
    // 생산 중 레시피가 변경됐을 때를 위해 현재 활성화된 레시피를 저장
    private RecipeDataSO activeRecipe;

    private float elapsedTime;
    private float bonusProductionChance;
    private int pendingOutputAmount;

    // 스킬 효과가 반영된 생산 시간 
    private float effectiveDuration;
    private bool isEnabled;
    private bool isRefreshing;

    // 0 : 감소 X
    // 0.2 : 생산시간 20% 감소
    private float productionTimeReductionRatio;

    public ProductionState State { get; private set; }

    public RecipeDataSO SelectedRecipe => selectedRecipe;
    public RecipeDataSO ActiveRecipe => activeRecipe;

    public float EffectiveDuration => effectiveDuration;

    public bool IsBusy => activeRecipe != null;

    public float RemainingTime
    {
        get
        {
            if (activeRecipe == null) return 0f;
            if (State == ProductionState.WaitingForOutputSpace) return 0f;

            return Mathf.Max(0f, effectiveDuration - elapsedTime);
        }
    }

    public float Progress
    {
        get
        {
            if (activeRecipe == null) return 0f;
            if (State == ProductionState.WaitingForOutputSpace) return 1f;

            return Mathf.Clamp01(elapsedTime / Mathf.Max(0.01f, effectiveDuration));
        }
    }

    public event Action<ProductionState> StateChanged;
    public event Action<RecipeDataSO> RecipeChanged;
    public event Action<RecipeDataSO> ProductionStarted;
    public event Action<RecipeDataSO> ProductionComplete;
    public event Action<float> ProgressChanged;

    public ProductionMachine(ItemInventory inputInventory, ItemInventory outputInventory)
    {
        this.inputInventory = inputInventory;
        this.outputInventory = outputInventory;

        State = ProductionState.Disable;
    }

    // 생산할 레시피를 변경
    public bool TrySetRecipe(RecipeDataSO recipe)
    {
        if (recipe == null) return false;

        if (selectedRecipe == recipe) return false;

        selectedRecipe = recipe;

        RecipeChanged?.Invoke(selectedRecipe);

        Refresh();

        return true;
    }

    public void SetEnable(bool enabled)
    {
        isEnabled = enabled;

        if (!enabled)
        {
            SetState(ProductionState.Disable);
            return;
        }

        Refresh();
    }

    // Producing 상태에서만 Tick 호출해 생산 진행
    public void Tick(float deltaTime)
    {
        if (!isEnabled || State != ProductionState.Producing || activeRecipe == null)
        {
            return;
        }

        elapsedTime += Mathf.Max(0f, deltaTime);
        elapsedTime = Mathf.Min(elapsedTime, effectiveDuration);

        ProgressChanged?.Invoke(Progress);

        if (elapsedTime >= effectiveDuration)
        {
            Refresh();
        }
    }
    
    // 현재 인벤토리와 작업 상태를 기준으로 결과물을 만들고, 다음 생산 시작을 시도
    public void Refresh() 
    {
        if (isRefreshing) return;

        isRefreshing = true;

        try
        {
            if (!isEnabled)
            {
                SetState(ProductionState.Disable);
                return;
            }

            // activeRecipe를 우선 처리
            if (activeRecipe != null)
            {
                if (elapsedTime < effectiveDuration)
                {
                    SetState(ProductionState.Producing);
                    return;
                }

                if (pendingOutputAmount <= 0)
                {
                    bool bonusSucceded = UnityEngine.Random.value < bonusProductionChance;

                    pendingOutputAmount = bonusSucceded ? 2 : 1;
                }

                // 생산 시간이 끝났다면 결과 인벤토리에 추가 시도
                // 인벤토리가 꽉차있다면 0 반환
                int added = outputInventory.Add(activeRecipe.Output, pendingOutputAmount);
                pendingOutputAmount -= added;

                // 생산물이 아직 남아있다면 대기 모드
                if (pendingOutputAmount > 0)
                {
                    SetState(ProductionState.WaitingForOutputSpace);
                    return;
                }

                RecipeDataSO completedRecipe = activeRecipe;

                activeRecipe = null;
                elapsedTime = 0f;
                effectiveDuration = 0f;
                pendingOutputAmount = 0;

                ProgressChanged?.Invoke(0f);
                ProductionComplete?.Invoke(completedRecipe);
            }

            if (selectedRecipe == null)
            {
                SetState(ProductionState.Idle);
                return;
            }

            bool hasMaterial = inputInventory.Contains(selectedRecipe.Input, 1);

            if (!hasMaterial)
            {
                SetState(ProductionState.WaitingForMaterials);
                return;
            }

            // 생산 시점에 재료 1개 소비
            // 실제 감소량이 없다면 0 반환
            int removed = inputInventory.Remove(selectedRecipe.Input, 1);

            // 소비한 재료가 1이 아니라면 재료 보충까지 대기
            if (removed != 1)
            {
                SetState(ProductionState.WaitingForMaterials);
                return;
            }

            activeRecipe = selectedRecipe;
            // 현재 생산 중인 레시피에 스킬 효과를 적용
            effectiveDuration = GetEffectiveDuration(activeRecipe);
            elapsedTime = 0f;

            SetState(ProductionState.Producing);

            ProgressChanged?.Invoke(0f);
            ProductionStarted?.Invoke(activeRecipe);
        } // end of try
        finally
        {
            isRefreshing = false;
        }
    }

    private void SetState(ProductionState newState)
    {
        if (State == newState) return;

        State = newState;
        StateChanged?.Invoke(State);
    }

    public void SetProductionSpeedMultiplier(float reductionRatio)
    {
        // 최대 95%까지만 감소 가능
        productionTimeReductionRatio = Mathf.Clamp(reductionRatio, 0f, 0.95f);
    }

    public float GetEffectiveDuration(RecipeDataSO recipe)
    {
        if (recipe == null) return 0;

        // ex) ratio = 0.2 -> durationMultiplier = 0.8
        float durationMultiplier = 1f - productionTimeReductionRatio;

        // ex) 6초 * 0.8 = 5초
        return Mathf.Max(0.01f, recipe.ProductionTime * durationMultiplier);
    }

    public void SetBonusProductionChance(float chanceRatio)
    {
        bonusProductionChance = Mathf.Clamp01(chanceRatio);
    }
}
