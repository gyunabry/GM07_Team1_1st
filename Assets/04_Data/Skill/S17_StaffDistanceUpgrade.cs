using UnityEngine;

[CreateAssetMenu(fileName = "205_StaffDistanceUpgradeEffect", menuName = "Skill Tree/Effects/205_StaffDistanceUpgrade")]
public class S17_StaffDistanceUpgrade : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.employeeManager.SetHunterAttackRangeIncreasePercent(skillData.value[nowLevel]);
    }
}
