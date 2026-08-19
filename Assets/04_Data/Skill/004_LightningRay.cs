using UnityEngine;

[CreateAssetMenu(fileName = "004_LightningRayEffect", menuName = "Skill Tree/Effects/004_LightningRay")]
public class LightningRay_004 : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        foreach (var unlock in context.playerAttack.attackUnlockDatas)
        {
            if (unlock.attackID == skillData.skillID)
            {
                unlock.unlock = true;
            }
        }
    }
}
