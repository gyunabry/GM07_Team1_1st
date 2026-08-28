using UnityEngine;

[CreateAssetMenu(fileName = "201_1_StaffProcessingSpeedEffect", menuName = "Skill Tree/Effects/201_1_StaffProcessingSpeed")]
public class S25_StaffProcessingSpeed : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.employeeManager.SetAllEmployeeProcessingSpeedIncreasePercent(skillData.value[nowLevel]);
    }
}
