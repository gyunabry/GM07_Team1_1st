using UnityEngine;

[CreateAssetMenu(fileName = "431_FireCircleAttackSpeed", menuName = "Skill Tree/SkillEffects/431_FireCircleAttackSpeed")]
public class S49_FireCircleAttackSpeed : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.playerAttack.upgrade[2].attackSpeed += skillData.value[nowLevel];
    }
}
