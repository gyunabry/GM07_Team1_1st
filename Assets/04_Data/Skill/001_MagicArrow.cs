using UnityEngine;

[CreateAssetMenu(fileName = "001_MagicArrowEffect", menuName = "Skill Tree/Effects/001_MagicArrow")]
public class MagicArrow_001 : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        foreach (var unlock in context.playerAttack.attackUnlockDatas)
        {
            if(unlock.attackID == skillData.skillID)
            {
                unlock.unlock = true;
            }
        }
    }
}
