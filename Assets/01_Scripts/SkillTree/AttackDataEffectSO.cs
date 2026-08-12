using UnityEngine;

[CreateAssetMenu(fileName = "AttackDataEffect", menuName = "Skill Tree/Effects/AttackData")]
public class AttackDataEffectSO : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        foreach (var attack in context.playerAttack.attackUnlockDatas)
        {
            if(attack.attackID == (int)skillData.value[0])
            {
                attack.unlock = true;
            }
        }
    }
}
