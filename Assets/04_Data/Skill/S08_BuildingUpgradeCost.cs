using UnityEngine;

[CreateAssetMenu(fileName = "S08_BuildingUpgradeCostEffect", menuName = "Skill Tree/Effects/S08_BuildingUpgradeCost")]
public class S08_BuildingUpgradeCost : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.economyModifierService.AddDiscount(skillData.value[nowLevel]);
    }
}
