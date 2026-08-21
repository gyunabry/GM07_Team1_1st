using UnityEngine;

[CreateAssetMenu(fileName = "002_FireCircleEffect", menuName = "Skill Tree/Effects/002_FireCircle")]
public class S47_FireCircle : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        foreach (var unlock in context.playerAttack.attackUnlockDatas)
        {
            if (unlock.attackID == skillData.skillID)
            {
                Debug.Log(unlock.attackID);
                Debug.Log(skillData.skillID);
                unlock.unlock = true;
            }
        }
    }
}
