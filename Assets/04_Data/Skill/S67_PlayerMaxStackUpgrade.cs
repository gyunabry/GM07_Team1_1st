using UnityEngine;

[CreateAssetMenu(fileName = "404_PlayerMaxStackUpgrade", menuName = "Skill Tree/Effects/404_PlayerMaxStackUpgrade")]
public class S67_PlayerMaxStackUpgrade: SkillEffectSO
{
    public override void SkillEffect(SkillEffectContext context, SkillDataSO skillData, int nowLevel)
    {
        // GetComponent는 추후 context로 이동
        PlayerInventory playerInventory = context.player.GetComponent<PlayerInventory>();
        playerInventory.Inventory.AddBonusCapacity((int)skillData.value[nowLevel]);
    }
}
