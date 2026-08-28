using UnityEngine;

[CreateAssetMenu(fileName = "S12_StaffAttackUpgrade", menuName = "Skill Tree/Effects/S12_StaffAttackUpgrade")]
public class S12_StaffAttackUpgrade : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        Debug.Log(nowLevel);
        Debug.Log(skillData.value[nowLevel]);
        context.employeeManager.SetHunterAttackRangeIncreasePercent(skillData.value[nowLevel]);
    }
}
