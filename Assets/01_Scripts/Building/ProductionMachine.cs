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
    private bool isEnabled;
    private bool isRefreshing;

    public ProductionState State { get; private set; }

    public RecipeDataSO SelectedRecipe => selectedRecipe;
    public RecipeDataSO ActiveRecipe => activeRecipe;

    public bool IsBusy => activeRecipe != null;
    public float Progress
    {
        get
        {
            if (activeRecipe == null) return 0f;

            if (State == ProductionState.WaitingForOutputSpace) return 1f;

            return Mathf.Clamp01(elapsedTime / activeRecipe.ProductionTime);
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
        if (IsBusy) return false;

        if (recipe == null) return false;

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

        float duration = Mathf.Max(0.01f, activeRecipe.ProductionTime);

        elapsedTime += Mathf.Max(0f, deltaTime);
        elapsedTime = Mathf.Min(elapsedTime, duration);

        ProgressChanged?.Invoke(Progress);

        if (elapsedTime >= duration)
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
                float duration = Mathf.Max(0.01f, activeRecipe.ProductionTime);

                if (elapsedTime < duration)
                {
                    SetState(ProductionState.Producing);
                    return;
                }

                // 생산 시간이 끝났다면 결과 인벤토리에 추가 시도
                // 인벤토리가 꽉차있다면 0 반환
                int added = outputInventory.Add(activeRecipe.Output, 1);

                if (added != 1)
                {
                    SetState(ProductionState.WaitingForOutputSpace);
                    return;
                }

                RecipeDataSO completedRecipe = activeRecipe;

                activeRecipe = null;
                elapsedTime = 0f;

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
}
