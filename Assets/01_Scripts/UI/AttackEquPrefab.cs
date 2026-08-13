using UnityEngine;

public class AttackEquPrefab : MonoBehaviour
{
    public PlayerAttack playerAttack;
    public AttackEquHud attackEquHud;
    public int attackID;
    public int equID;
    public int slotIndex;

    public void EquipSkill()
    {
        foreach (var attack in playerAttack.attackUnlockDatas)
        {
            if (attack.attackID == attackID)
            {
                attackEquHud.EquipSlot(attack.attackID);
                attackEquHud.OnOffButton();
            }
        }
    }
}
