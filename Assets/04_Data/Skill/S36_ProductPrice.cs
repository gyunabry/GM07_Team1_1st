using UnityEngine;

[CreateAssetMenu(fileName = "303_ProductPriceEffect", menuName = "Skill Tree/Effects/303_ProductPrice")]
public class S36_ProductPrice : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.economyModifierService.AddGlobalProductBonusRatio(skillData.value[nowLevel]);
    }
}
