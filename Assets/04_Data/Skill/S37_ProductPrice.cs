using UnityEngine;

[CreateAssetMenu(fileName = "303_1_ProductPriceEffect", menuName = "Skill Tree/Effects/303_1_ProductPrice")]
public class S37_ProductPrice : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.economyModifierService.AddGlobalProductBonusRatio(skillData.value[nowLevel]);
    }
}
