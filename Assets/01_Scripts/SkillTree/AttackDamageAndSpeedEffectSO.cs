using UnityEngine;

[CreateAssetMenu(fileName = "AttackDamageAndSpeedEffect", menuName = "Skill Tree/Effects/Attack Damage&Speed")]
public class AttackDamageAndSpeedEffectSO : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        float damage = skillData.multiValue[nowLevel - 1].value[0];
        float speed = skillData.multiValue[nowLevel - 1].value[1];
        context.player.AttackDamage += damage;
        context.player.AttackSpeed += speed;
    }
}
