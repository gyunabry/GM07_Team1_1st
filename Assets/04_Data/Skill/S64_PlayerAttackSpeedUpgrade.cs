using UnityEngine;

[CreateAssetMenu(fileName = "402_2_PlayerAttackSpeedUpgrade", menuName = "Skill Tree/Effects/402_2_PlayerAttackSpeedUpgrade")]
public class S64_PlayerAttackSpeedUpgrade : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.player.attackSpeed += skillData.value[nowLevel];
    }
}
