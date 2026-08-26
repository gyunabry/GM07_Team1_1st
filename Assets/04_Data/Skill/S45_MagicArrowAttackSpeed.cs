using UnityEngine;

[CreateAssetMenu(fileName = "421_MagicArrowAttackSpeed", menuName = "Skill Tree/SkillEffects/421_MagicArrowAttackSpeed")]
public class S45_MagicArrowAttackSpeed : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.playerAttack.upgrade[1].attackSpeed += skillData.value[nowLevel];
    }
}
