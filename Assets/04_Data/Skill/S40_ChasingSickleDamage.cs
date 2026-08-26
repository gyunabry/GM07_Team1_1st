using UnityEngine;

[CreateAssetMenu(fileName = "410_ChasingSickleDamage", menuName = "Skill Tree/SkillEffects/410_ChasingSickleDamage")]
public class S40_ChasingSickleDamage : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.playerAttack.upgrade[0].damage += skillData.value[nowLevel];
    }
}
