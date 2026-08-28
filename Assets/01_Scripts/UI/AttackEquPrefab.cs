using UnityEngine;
using UnityEngine.UI;

public class AttackEquPrefab : MonoBehaviour
{
    [SerializeField] private Image skillIcon;

    private AttackEquHud ownerSlot;
    private string attackID;
    private bool unlock;

    //public PlayerAttack playerAttack;
    //public AttackEquHud attackEquHud;
    // public string attackID;
    //public string equID;
    //public int slotIndex;

    public void Bind(AttackEquHud slot, string id, Sprite sprite, bool unlocked)
    {
        ownerSlot = slot;
        attackID = id;
        unlock = unlocked;

        if (skillIcon != null)
        {
            skillIcon.sprite = sprite;
        }
        if(unlocked == true)
        {
            RectTransform[] rect = GetComponentsInChildren<RectTransform>();
            if(rect.Length > 6)
            {
                rect[6].gameObject.SetActive(false);
            }
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
        if (unlock == false)
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
