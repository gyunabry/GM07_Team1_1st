using UnityEngine;
using UnityEngine.UI;

public class AttackEquHud : MonoBehaviour
{
    Button button;
    [SerializeField] PlayerAttack playerAttack;
    [SerializeField] MonsterPoolManager monsterPoolManager;
    [SerializeField] GameObject layoutWidth;
    public int equAttackID;
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
    public void EquipSlot(int id)
    {
        playerAttack.StartAndStopAttackCo(slotIndex, id, this);
    }
    public void EquipRefresh(int id)
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
            Debug.Log("¿Â¬¯ «ÿ¡¶");
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
