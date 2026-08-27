using UnityEngine;

[CreateAssetMenu(fileName = "303_2_ProductPriceEffect", menuName = "Skill Tree/Effects/303_2_ProductPrice")]
public class S38_ProductPrice : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.economyModifierService.AddGlobalProductBonusRatio(skillData.value[nowLevel]);
    }
}
