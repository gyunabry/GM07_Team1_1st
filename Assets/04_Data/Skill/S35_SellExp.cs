using UnityEngine;

[CreateAssetMenu(fileName = "307_SellExpEffect", menuName = "Skill Tree/Effects/307_SellExp")]
public class S35_SellExp : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        RewardSkillRegistry.AddSellExperienceBonusChance(skillData.value[nowLevel]);
    }
}
