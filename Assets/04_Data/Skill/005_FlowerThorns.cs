using UnityEngine;

[CreateAssetMenu(fileName = "005_FlowerThornsEffect", menuName = "Skill Tree/Effects/005_FlowerThorns")]
public class FlowerThorns_005 : SkillEffectSO
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
