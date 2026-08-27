using UnityEngine;

[CreateAssetMenu(fileName = "003_ChasingSickleEffect", menuName = "Skill Tree/Effects/003_ChasingSickle")]
public class S39_ChasingSickle : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        foreach (var unlock in context.playerAttack.attackUnlockDatas)
        {
            Debug.Log(unlock.attackID);
            Debug.Log(skillData.skillID);
            if (unlock.attackID == skillData.skillID)
            {
                Debug.Log("언락 실행");
                unlock.unlock = true;
            }
        }
    }
}
