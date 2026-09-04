using UnityEngine;

[CreateAssetMenu(fileName = "206_CarryoutStaffCarryoutAmountEffect", menuName = "Skill Tree/Effects/206_CarryoutStaffCarryoutAmount")]
public class S13_CarryoutStaffCarryoutAmount : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        if (context == null || context.employeeManager == null || skillData == null ||
            skillData.value == null || nowLevel < 0 || nowLevel >= skillData.value.Length)
        {
            return;
        }

        int capacityBonus = Mathf.Max(0, Mathf.RoundToInt(skillData.value[nowLevel]));
        context.employeeManager.SetCarrierCarryingCapacityBonus(capacityBonus);
    }
}
