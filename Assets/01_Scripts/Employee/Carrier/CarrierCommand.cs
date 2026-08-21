/// <summary>
/// 운반 명령의 실제 대상은 생산 아이템이 아닌 생산 건물입니다.
/// </summary>
public readonly struct CarrierCommand
{
    public CarrierCommandType Type { get; }
    public ProductionBuilding TargetBuilding { get; }
    public RecipeDataSO AssignedRecipe { get; }

    public bool IsValid => TargetBuilding != null && AssignedRecipe != null;

    public CarrierCommand(CarrierCommandType type, ProductionBuilding targetBuilding)
    {
        Type = type;
        TargetBuilding = targetBuilding;
        AssignedRecipe = targetBuilding != null ? targetBuilding.SelectedRecipe : null;
    }
}
