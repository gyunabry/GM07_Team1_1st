using System;
using UnityEngine;

/*
 [생산 건물 프로세스]

1. 플레이어가 UI를 통해 레시피 선택
2. 플레이어 인벤토리에서 생산 건물에 해당 레시피의 Input 재료 자동 납품
3. 레시피 시간에 맞춰 가공 시작
4. 시간이 모두 지나면 건물 인벤토리에 아이템 추가 및 시각화

- 만약 생산 중 레시피를 변경한다면 현재 작업 진행 중인 아이템 가공 완료 후 변경(예약 기능)
- 
 */

public class ProductionBuilding : MonoBehaviour
{
    [SerializeField] private RecipeDataSO initialRecipe;

    [Header("생산 인벤토리")]
    [SerializeField] private ItemInventory inputInventory = new();
    [Header("출력 인벤토리")]
    [SerializeField] private ItemInventory outputInventory = new();

    // 실제 생산 규칙과 상태를 관리하는 클래스
    private ProductionMachine machine;

    private PlacedBuilding placedBuilding;

    #region 프로퍼티
    public RecipeDataSO SelectedRecipe => machine.SelectedRecipe;
    public RecipeDataSO ActiveRecipe => machine.ActiveRecipe;
    public ItemInventory InputInventory => inputInventory;
    public ItemInventory OutputInventory => outputInventory;
    public ProductionState State => machine.State;
    public float Progress => machine.Progress;
    public bool IsBusy => machine.IsBusy;

    // 설치 완료 판정
    public bool CanOperate => placedBuilding != null && placedBuilding.IsComplete;
    #endregion

    public event Action<ProductionState> StateChanged;
    public event Action<RecipeDataSO> RecipeChanged;
    public event Action<RecipeDataSO> ProductionStarted;
    public Action<RecipeDataSO> ProductionComplete;
    public event Action<float> ProgressChanged;

    private void Awake()
    {
        placedBuilding = GetComponent<PlacedBuilding>();

        machine = new ProductionMachine(inputInventory, outputInventory);

        if (initialRecipe != null) machine.TrySetRecipe(initialRecipe);
    }

    private void OnEnable()
    {
        SubscribeInventoryEvents();
        SubscribeMachineEvents();

        machine.SetEnable(true);
    }

    private void OnDisable()
    {
        UnsubscribeInventoryEvents();
        UnsubscribeMachineEvents();

        machine.SetEnable(false);
    }

    private void Update()
    {
        // 상태 검사는 ProductionMachine 내부에서 진행
        machine.Tick(Time.deltaTime);
    }

    // UI에서 호출할 레시피 선택 메서드
    public bool TrySetRecipe(RecipeDataSO recipe)
    {
        return machine.TrySetRecipe(recipe);
    }

    #region 이벤트 구독
    private void SubscribeInventoryEvents()
    {
        inputInventory.InventoryChanged += HandleInventoryChanged;
        outputInventory.InventoryChanged += HandleInventoryChanged;
    }

    private void UnsubscribeInventoryEvents()
    {
        inputInventory.InventoryChanged -= HandleInventoryChanged;
        outputInventory.InventoryChanged -= HandleInventoryChanged;
    }

    //private void SubscribeBuildingEvents()
    //{
    //    placedBuilding.OnStateChanged += HandleBuildingStateChanged;
    //}
    
    //private void UnsubscribeBuildingEvents()
    //{
    //    placedBuilding.OnStateChanged -= HandleBuildingStateChanged;
    //}

    private void SubscribeMachineEvents()
    {
        machine.StateChanged += HandleStateChanged;
        machine.RecipeChanged += HandleRecipeChanged;
        machine.ProductionStarted += HandleProductionStarted;
        machine.ProductionComplete += HandleProductionCompleted;
        machine.ProgressChanged += HandleProgressChanged;
    }

    private void UnsubscribeMachineEvents()
    {
        machine.StateChanged -= HandleStateChanged;
        machine.RecipeChanged -= HandleRecipeChanged;
        machine.ProductionStarted -= HandleProductionStarted;
        machine.ProductionComplete -= HandleProductionCompleted;
        machine.ProgressChanged -= HandleProgressChanged;
    }

    #endregion

    // Input 및 Output 변경 검사
    private void HandleInventoryChanged()
    {
        machine.Refresh();
    }

    private void HandleStateChanged(ProductionState newState)
    {
        StateChanged?.Invoke(newState);
    }

    private void HandleRecipeChanged(RecipeDataSO recipe)
    {
        RecipeChanged?.Invoke(recipe);
    }

    private void HandleProductionStarted(RecipeDataSO recipe)
    {
        ProductionStarted?.Invoke(recipe);
    }

    private void HandleProductionCompleted(RecipeDataSO recipe)
    {
        ProductionComplete?.Invoke(recipe);
    }

    private void HandleProgressChanged(float progress)
    {
        ProgressChanged?.Invoke(progress);
    }
}
