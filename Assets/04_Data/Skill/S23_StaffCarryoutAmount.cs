using UnityEngine;

[CreateAssetMenu(fileName = "203_StaffCarryoutAmountEffect", menuName = "Skill Tree/Effects/203_StaffCarryoutAmount")]
public class S23_StaffCarryoutAmount : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        if (context == null || context.employeeManager == null || skillData == null ||
            skillData.value == null || nowLevel < 0 || nowLevel >= skillData.value.Length)
        {
            return;
        }

        int capacityBonus = Mathf.Max(0, Mathf.RoundToInt(skillData.value[nowLevel]));
        context.employeeManager.SetHunterCarryingCapacityBonus(capacityBonus);
        context.employeeManager.SetCarrierCarryingCapacityBonus(capacityBonus);
    }
}
