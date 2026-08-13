using UnityEngine;

[CreateAssetMenu(fileName = "AttackDamageEffect", menuName = "Skill Tree/Effects/Attack Damage")]
public class AttackDamageEffectSO : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        float damage = skillData.Value[nowLevel - 1];
        context.player.AttackDamage += damage;
    }
}
