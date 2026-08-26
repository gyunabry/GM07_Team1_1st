using UnityEngine;

[CreateAssetMenu(fileName = "401_PlayerAttackDamageUpgrade", menuName = "Skill Tree/Effects/401_PlayerAttackDamageUpgrade")]
public class S59_PlayerAttackDamageUpgrade : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.player.attackDamage += skillData.value[nowLevel];
    }
}
