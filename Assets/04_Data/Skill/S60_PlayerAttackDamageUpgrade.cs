using UnityEngine;

[CreateAssetMenu(fileName = "401_1_PlayerAttackDamageUpgrade", menuName = "Skill Tree/Effects/401_1_PlayerAttackDamageUpgrade")]
public class S60_PlayerAttackDamageUpgrade : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.player.attackDamage += skillData.value[nowLevel];
        context.characterPanelController.RefreshAll();
    }
}
