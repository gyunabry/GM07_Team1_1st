using UnityEngine;

[CreateAssetMenu(fileName = "306_HuntExpEffect", menuName = "Skill Tree/Effects/306_HuntExp")]
public class S34_HuntExp : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        RewardSkillRegistry.AddHuntExperienceBonusChance(skillData.value[nowLevel]);
    }
}
