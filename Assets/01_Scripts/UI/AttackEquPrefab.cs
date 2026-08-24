using UnityEngine;
using UnityEngine.UI;

public class AttackEquPrefab : MonoBehaviour
{
    [SerializeField] private Image skillIcon;

    private AttackEquHud ownerSlot;
    private string attackID;

    //public PlayerAttack playerAttack;
    //public AttackEquHud attackEquHud;
    // public string attackID;
    //public string equID;
    //public int slotIndex;

    public void Bind(AttackEquHud slot, string id, Sprite sprite)
    {
        ownerSlot = slot;
        attackID = id;

        if (skillIcon != null)
        {
            skillIcon.sprite = sprite;
        }
    }

    public void EquipSkill()
    {
        //foreach (var attack in playerAttack.attackUnlockDatas)
        //{
        //    if (attack.attackID == attackID)
        //    {
        //        attackEquHud.EquipSlot(attack.attackID);
        //        attackEquHud.OnOffButton();
        //    }
        //}

        if (ownerSlot == null || string.IsNullOrEmpty(attackID))
        {
            return;
        }

        AttackEquHud slot = ownerSlot;
        string selectedAttackId = attackID;

        slot.EquipSlot(selectedAttackId);
        slot.CloseSelector();
    }

    public void ResetState()
    {
        ownerSlot = null;
        attackID = null;

        if (skillIcon != null)
        {
            skillIcon.sprite = null;
        }
    }
}
