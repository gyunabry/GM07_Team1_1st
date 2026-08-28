using UnityEngine;

[CreateAssetMenu(fileName = "S01_ProductionBuildingStorageUpgradeEffect", menuName = "Skill Tree/Effects/S01_ProductionBuildingStorageUpgrade")]
public class S01_ProductionBuildingStorageUpgrade : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        ProductionSkillRegistry.AddStorageCapacityBonus((int)skillData.value[nowLevel]);
    }
}
