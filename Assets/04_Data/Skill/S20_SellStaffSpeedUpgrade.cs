using UnityEngine;

[CreateAssetMenu(fileName = "209_1_SellStaffSpeedUpgrade", menuName = "Skill Tree/Effects/209_1_SellStaffSpeedUpgrade")]
public class S20_SellStaffSpeedUpgrade : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.employeeManager.SetSalesPaymentTimeReductionPercent(skillData.value[nowLevel]);
    }
}
