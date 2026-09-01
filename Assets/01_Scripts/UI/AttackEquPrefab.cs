using UnityEngine;
using UnityEngine.UI;

public class AttackEquPrefab : MonoBehaviour
{
    [SerializeField] private Image skillIcon;

    private AttackEquHud ownerSlot;
    private string attackID;
    private bool unlock;
    private SkillDesc popUp;
    private PoolManager poolManager;
    private PlayerAttack playerAttack;

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
        poolManager = FindAnyObjectByType<PoolManager>();
        playerAttack = FindAnyObjectByType<PlayerAttack>();

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
    public void MouserEnter()
    {
        if (attackID == null) return;

        popUp = poolManager.GetPool<SkillDesc>();
        popUp.transform.SetParent(transform);
        RectTransform rect = popUp.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(-100f, -340f);
        float[] value = new float[4];
        value = playerAttack.ReturnSkillValue(attackID);
        popUp.SetDamage(value[0]);
        popUp.SetSpeed(value[1]);
        popUp.SetDistance(value[2]);
        popUp.SetProjectile((int)value[3]);
    }
    public void MouseExit()
    {
        if (popUp != null)
        {
            poolManager.ReturnPool(popUp);
        }
    }
}
