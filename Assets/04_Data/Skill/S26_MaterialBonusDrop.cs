using UnityEngine;

[CreateAssetMenu(fileName = "301_MaterialBonusDropEffect", menuName = "Skill Tree/Effects/301_MaterialBonusDrop")]
public class S26_MaterialBonusDrop : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        RewardSkillRegistry.AddMaterialBonusDropChance(skillData.value[nowLevel]);
    }
}
