using UnityEngine;

[CreateAssetMenu(fileName = "411_ChasingSickleAttackSpeed", menuName = "Skill Tree/SkillEffects/411_ChasingSickleAttackSpeed")]
public class S41_ChasingSickleAttackSpeed : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.playerAttack.upgrade[0].attackSpeed += skillData.value[nowLevel];
    }
}
