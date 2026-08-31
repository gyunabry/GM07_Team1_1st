using UnityEngine;

[CreateAssetMenu(fileName = "208_StaffAttackSpeedUpgrade", menuName = "Skill Tree/Effects/208_StaffAttackSpeedUpgrade")]
public class S15_StaffAttackSpeedUpgrade : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.employeeManager.SetHunterAttackIntervalReductionPercent(skillData.value[nowLevel]);
    }
}
