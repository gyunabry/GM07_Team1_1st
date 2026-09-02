using UnityEngine;

[CreateAssetMenu(fileName = "003_ChasingSickleEffect", menuName = "Skill Tree/Effects/003_ChasingSickle")]
public class S39_ChasingSickle : SkillEffectSO
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
