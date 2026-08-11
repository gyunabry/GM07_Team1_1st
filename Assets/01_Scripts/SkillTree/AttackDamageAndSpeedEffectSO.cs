using UnityEngine;

[CreateAssetMenu(fileName = "AttackDamageAndSpeedEffect", menuName = "Skill Tree/Effects/Attack Damage&Speed")]
public class AttackDamageAndSpeedEffectSO : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        float damage = skillData.Value[nowLevel - 1];
        context.player.AttackDamage += damage;
    }
}
