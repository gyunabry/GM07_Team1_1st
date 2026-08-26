using UnityEngine;

[CreateAssetMenu(fileName = "402_PlayerAttackSpeedUpgrade", menuName = "Skill Tree/Effects/402_PlayerAttackSpeedUpgrade")]
public class S62_PlayerAttackSpeedUpgrade: SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.player.attackSpeed += skillData.value[nowLevel];
    }
}
