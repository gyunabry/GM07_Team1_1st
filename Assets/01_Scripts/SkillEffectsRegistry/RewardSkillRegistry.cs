using UnityEngine;

// 몬스터 처치 시 획득하는 경험치 및 아이템 추가 획득 찬스
// 아이템 판매 시 획득하는 경험치 증가

public struct RewardSkillModifiers
{
    public float MaterialBonusDropChance;
    public float HuntExperienceBonusRatio;
    public float SellExperienceBonusRatio;

    public static RewardSkillModifiers Default = default;
}

public class RewardSkillRegistry : MonoBehaviour
{
    private static RewardSkillModifiers current;
    private static RewardSkillModifiers pending;

    public static RewardSkillModifiers Current => current;

    public static void BeginRebuild()
    {
        pending = RewardSkillModifiers.Default;
    }

    public static void AddMaterialBonusDropChance(float percent)
    {
        pending.MaterialBonusDropChance += Mathf.Max(0f, percent) / 100f;
    }

    public static void AddHuntExperienceBonusChance(float percent)
    {
        pending.HuntExperienceBonusRatio += Mathf.Max(0f, percent) / 100f;
    }

    public static void AddSellExperienceBonusChance(float percent)
    {
        pending.SellExperienceBonusRatio += Mathf.Max(0f, percent) / 100f;
    }

    public static void Commit()
    {
        pending.MaterialBonusDropChance = Mathf.Clamp01(pending.MaterialBonusDropChance);
        pending.HuntExperienceBonusRatio = Mathf.Clamp01(pending.HuntExperienceBonusRatio);
        pending.SellExperienceBonusRatio = Mathf.Clamp01(pending.SellExperienceBonusRatio);

        current = pending;
    }

    // 적을 잡을 때마다 호출해 추가 아이템 획득 여부를 검사
    public static bool RollAdditionalItemDrop()
    {
        return CheckAdditionalItemDrop(Random.value);
    }

    // 아이템 추가 드랍 확률이 랜덤 값보다 큰지 검사
    // 크다면 참 반환
    public static bool CheckAdditionalItemDrop(float randomValue)
    {
        return Mathf.Clamp01(randomValue) < current.MaterialBonusDropChance;
    }

    public static int ApplyHuntExperience(int baseExp)
    {
        return ApplyExperience(baseExp, current.HuntExperienceBonusRatio);
    }

    public static int ApplySellExperience(int baseExp)
    {
        return ApplyExperience(baseExp, current.SellExperienceBonusRatio);
    }

    private static int ApplyExperience(int baseExp, float bonusRatio)
    {
        if (baseExp <= 0) return 0;

        return Mathf.Max(0, Mathf.RoundToInt(baseExp * (1f + bonusRatio)));
    }
    
    private static void ResetRuntimeState()
    {
        current = RewardSkillModifiers.Default;
        pending = RewardSkillModifiers.Default;
    }
}
