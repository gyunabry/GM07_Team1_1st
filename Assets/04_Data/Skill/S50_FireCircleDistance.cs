using UnityEngine;

[CreateAssetMenu(fileName = "432_FireCircleDistance", menuName = "Skill Tree/SkillEffects/432_FireCircleDistance")]
public class S50_FireCircleDistance : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.playerAttack.upgrade[2].distance += skillData.value[nowLevel];
    }
}
