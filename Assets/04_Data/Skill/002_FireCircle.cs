using UnityEngine;

[CreateAssetMenu(fileName = "002_FireCircleEffect", menuName = "Skill Tree/Effects/002_FireCircle")]
public class FireCircle_002 : SkillEffectSO
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
