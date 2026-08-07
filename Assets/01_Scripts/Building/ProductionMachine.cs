using System;
using UnityEngine;

// 생산 상태 
public enum ProductionState
{
    Disable,                // 건설 중 or 비활성 상태
    Idle,                   // 레시피 미선택
    Ready,                  // 레시피 선택 완료
    WaitingForMaterials,    // 재료 부족
    WaitingForOutputSpace,  // 결과물 공간 부족
    Producing               // 생산 중
}

public class ProductionMachine
{
    public ProductionState State { get; private set; }
    public event Action<ProductionState> StateChanged;

    public ProductionState Evaluate(
        bool isEnabled,
        bool hasRecipe,
        bool isProducing,
        bool hasMaterials,
        bool hasOutputStorage
    );
}
