using UnityEngine;

[CreateAssetMenu(fileName = "AttackSpeedEffect", menuName = "Skill Tree/Effects/Attack Speed")]
public class AttackSpeedEffectSO : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        float attackSpeed = skillData.Value[nowLevel - 1];
        context.player.AttackSpeed += attackSpeed;
    }
}

