using UnityEngine;
using UnityEngine.UI;

public class AttackEquHud : MonoBehaviour
{
    Button button;
    [SerializeField] PlayerAttack playerAttack;
    [SerializeField] MonsterPoolManager monsterPoolManager;
    [SerializeField] GameObject layoutWidth;

    [SerializeField] AttackEquHud closeButton1;
    [SerializeField] AttackEquHud closeButton2;
    public string equAttackID;
    public bool equip;
    public int slotIndex;

    private void Awake()
    {
        button = GetComponent<Button>();
        layoutWidth.SetActive(false);
    }
    public void OnOffButton()
    {
        if (layoutWidth.activeSelf == true)
        {
            AttackEquPrefab[] childButton = GetComponentsInChildren<AttackEquPrefab>();
            foreach(var button in childButton)
            {
                monsterPoolManager.ReturnPool(button);
            }
            layoutWidth.SetActive(false);
        }
        else
        {
            layoutWidth.SetActive(true);
            if(closeButton1.layoutWidth.activeSelf == true)
            {
                closeButton1.OnOffButton();
            }
            if (closeButton2.layoutWidth.activeSelf == true)
            {
                closeButton2.OnOffButton();
            }
            SelectAttack();
        }
    }
    public void SelectAttack()
    {
        foreach(var attackData in playerAttack.attackUnlockDatas)
        {
            if(attackData.unlock == true)
            {
                AttackEquPrefab prefabButton = monsterPoolManager.GetPool<AttackEquPrefab>();
                Button prefabImage = prefabButton.GetComponent<Button>();
                prefabButton.attackEquHud = this;
                prefabImage.image.sprite = attackData.sprite;
                prefabButton.transform.SetParent(layoutWidth.transform);
                prefabButton.equID = equAttackID;
                prefabButton.slotIndex = slotIndex;
                prefabButton.playerAttack = playerAttack;
                prefabButton.attackID = attackData.attackID;
            }
        }
    }
    public void EquipSlot(string id)
    {
        playerAttack.StartAndStopAttackCo(slotIndex, id, this);
    }
    public void EquipRefresh(string id)
    {
        equAttackID = id;
        equip = false;
        foreach(var playerAttackSlot in playerAttack.slots)
        {
            if(playerAttackSlot.equipAttackID == id)
            {
                equip = true;
            }
        }
        if(equip == false)
        {
            button.image.sprite = null;
        }
        else
        {
            foreach(var unlockData in playerAttack.attackUnlockDatas)
            {
                if(unlockData.attackID == id)
                {
                    button.image.sprite = unlockData.sprite;
                }
            }
        }
    }
}
