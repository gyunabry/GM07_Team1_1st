using UnityEngine;

[CreateAssetMenu(fileName = "S09_StorageMiniUpgradeEffect", menuName = "Skill Tree/Effects/S09_StorageMiniUpgrade")]
public class S09_StorageMiniUpgrade : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        StorageSkillRegistry.AddCapacityBonus((int)skillData.value[nowLevel]);
    }
}
