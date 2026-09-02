using UnityEngine;

[CreateAssetMenu(fileName = "403_PlayerMoveSpeedUpgrade", menuName = "Skill Tree/Effects/403_PlayerMoveSpeedUpgrade")]
public class S65_PlayerMoveSpeedUpgrade: SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.player.navMeshAgent.speed += skillData.value[nowLevel];
        context.characterPanelController.RefreshAll();
    }
}
