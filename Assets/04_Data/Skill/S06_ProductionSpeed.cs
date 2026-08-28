using UnityEngine;

[CreateAssetMenu(fileName = "S06_ProductionSpeedEffect", menuName = "Skill Tree/Effects/S06_ProductionSpeed")]

public class S06_ProductionSpeed : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        ProductionSkillRegistry.AddProductionTimeReduction(skillData.value[nowLevel]);
    }
}
