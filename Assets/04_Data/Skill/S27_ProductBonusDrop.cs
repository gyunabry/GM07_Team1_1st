using UnityEngine;

[CreateAssetMenu(fileName = "302_ProductBonusDropEffect", menuName = "Skill Tree/Effects/302_ProductBonusDrop")]
public class S27_ProductBonusDrop : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        ProductionSkillRegistry.AddBonusProductionChance(skillData.value[nowLevel]);
    }
}
