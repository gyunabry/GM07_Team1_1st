using UnityEngine;

[CreateAssetMenu(fileName = "405_1_PlayerItemGetRange", menuName = "Skill Tree/Effects/405_1_PlayerItemGetRange")]
public class S70_PlayerItemGetRange : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        PlayerItemCollector itemCollector = context.player.GetComponentInChildren<PlayerItemCollector>();
        itemCollector.AddRangeBonusRate(skillData.value[nowLevel]);
    }
}
