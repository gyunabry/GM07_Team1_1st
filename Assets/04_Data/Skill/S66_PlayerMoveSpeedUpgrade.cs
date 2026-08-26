using UnityEngine;

[CreateAssetMenu(fileName = "403_1_PlayerMoveSpeedUpgrade", menuName = "Skill Tree/Effects/403_1_PlayerMoveSpeedUpgrade")]
public class S66_PlayerMoveSpeedUpgrade : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.player.navMeshAgent.speed += skillData.value[nowLevel];
    }
}
