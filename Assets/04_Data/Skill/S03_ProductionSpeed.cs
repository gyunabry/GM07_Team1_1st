using UnityEngine;

[CreateAssetMenu(fileName = "S03_ProductionSpeedEffect", menuName = "Skill Tree/Effects/S03_ProductionSpeed")]
public class S03_ProductionSpeed : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        ProductionSkillRegistry.AddProductionTimeReduction(skillData.value[nowLevel]);
    }
}
