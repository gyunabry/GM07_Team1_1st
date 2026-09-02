using UnityEngine;

[CreateAssetMenu(fileName = "402_1_PlayerAttackSpeedUpgrade", menuName = "Skill Tree/Effects/402_1_PlayerAttackSpeedUpgrade")]
public class S63_PlayerAttackSpeedUpgrade : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.player.attackSpeed += skillData.value[nowLevel];
        context.characterPanelController.RefreshAll();
    }
}
