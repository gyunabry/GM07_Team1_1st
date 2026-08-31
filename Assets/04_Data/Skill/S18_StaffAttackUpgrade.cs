using UnityEngine;

[CreateAssetMenu(fileName = "204_1_StaffAttackUpgrade", menuName = "Skill Tree/Effects/204_1_StaffAttackUpgrade")]
public class S18_StaffAttackUpgrade : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.employeeManager.SetHunterAttackDamageIncreasePercent(skillData.value[nowLevel]);
    }
}
