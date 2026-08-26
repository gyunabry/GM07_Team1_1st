using UnityEngine;

[CreateAssetMenu(fileName = "442_LightningRayProjectileCount", menuName = "Skill Tree/SkillEffects/442_LightningRayProjectileCount")]
public class S54_LightningRayProjectileCount : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.playerAttack.upgrade[3].projectileCount += (int)skillData.value[nowLevel];
    }
}
