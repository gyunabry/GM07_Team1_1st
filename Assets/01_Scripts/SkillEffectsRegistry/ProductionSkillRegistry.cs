using System.Collections.Generic;
using UnityEngine;

public struct ProductionSkillModifiers
{
    public int StorageCapacityBonus;            // 저장소 공간 확장
    public float ProductionTimeReductionRatio;  // 생산 시간 감소
    public float ProductionBonusChance;         // 생산물 추가 생성 확률

    public static ProductionSkillModifiers Default => default;
}

public class ProductionSkillRegistry : MonoBehaviour
{
    private static readonly HashSet<ProductionBuilding> buildings = new();

    // 스킬 적용을 일괄적으로 하기 위해 분리
    private static ProductionSkillModifiers current;
    private static ProductionSkillModifiers pending;

    public static ProductionSkillModifiers Current => current;

    public static void Register(ProductionBuilding building)
    {
        if (building == null) return;

        buildings.Add(building);
        building.ApplySkillModifiers(current);
    }

    public static void Unregister(ProductionBuilding building)
    {
        if (building != null)
        {
            buildings.Remove(building);
        }
    }

    public static void BeginRebuild()
    {
        pending = ProductionSkillModifiers.Default;
    }

    public static void AddStorageCapacityBonus(int amount)
    {
        pending.StorageCapacityBonus += Mathf.Max(0, amount);
    }

    public static void AddProductionTimeReduction(float percent)
    {
        pending.ProductionTimeReductionRatio += Mathf.Abs(percent) / 100f;
    }

    public static void AddBonusProductionChance(float percent)
    {
        pending.ProductionBonusChance += Mathf.Abs(percent) / 100f;
    }

    public static void Commit()
    {
        // 상한 제한
        pending.ProductionTimeReductionRatio = Mathf.Clamp(pending.ProductionTimeReductionRatio, 0f, 0.95f);
        pending.ProductionBonusChance = Mathf.Clamp01(pending.ProductionBonusChance);

        current = pending;

        foreach (ProductionBuilding building in buildings)
        {
            if (building != null)
            {
                building.ApplySkillModifiers(current);
            }
        }
    }

    private static void ResetRuntimeState()
    {
        buildings.Clear();
        current = default;
        pending = default;
    }
}
