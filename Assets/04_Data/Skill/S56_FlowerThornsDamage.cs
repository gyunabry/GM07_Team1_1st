using UnityEngine;

[CreateAssetMenu(fileName = "450_FlowerThornsDamage", menuName = "Skill Tree/SkillEffects/450_FlowerThornsDamage")]
public class S56_FlowerThornsDamage : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.playerAttack.upgrade[4].damage += skillData.value[nowLevel];
    }
}
