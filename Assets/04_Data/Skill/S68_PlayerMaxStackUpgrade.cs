using UnityEngine;

[CreateAssetMenu(fileName = "404_1_PlayerMaxStackUpgrade", menuName = "Skill Tree/Effects/404_1_PlayerMaxStackUpgrade")]
public class S68_PlayerMaxStackUpgrade : SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        PlayerInventory playerInventory = context.player.GetComponent<PlayerInventory>();
        playerInventory.Inventory.AddBonusCapacity((int)skillData.value[nowLevel]);
    }
}
