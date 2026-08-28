using UnityEngine;

[CreateAssetMenu(fileName = "201_StaffProcessingSpeedEffect", menuName = "Skill Tree/Effects/201_StaffProcessingSpeed")]
public class S21_StaffProcessingSpeed : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.employeeManager.SetAllEmployeeProcessingSpeedIncreasePercent(skillData.value[nowLevel]);
    }
}
