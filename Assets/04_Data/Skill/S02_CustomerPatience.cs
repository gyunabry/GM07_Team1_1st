using UnityEngine;

[CreateAssetMenu(fileName = "S02_CustomerPatienceEffect", menuName = "Skill Tree/Effects/S02_CustomerPatience")]
public class S02_CustomerPatience : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        context.customerSpawnManager.SetPatienceIncreasePercent(skillData.value[nowLevel]);
    }
}
