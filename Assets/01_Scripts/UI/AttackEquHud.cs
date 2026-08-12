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

    private void Awake()
    {
        button = GetComponent<Button>();
        layoutWidth.SetActive(false);
    }
    public void SelectAttack()
    {
        layoutWidth.SetActive(true);
        foreach(var attackData in playerAttack.attackUnlockDatas)
        {
            if(attackData.unlock == true)
            {
                AttackEquPrefab prefabButton = monsterPoolManager.GetPool<AttackEquPrefab>();
                Button prefabImage = prefabButton.GetComponent<Button>();
                prefabButton.attackEquHud = this;
                prefabImage.image.sprite = attackData.sprite;
                prefabButton.transform.SetParent(layoutWidth.transform);
                prefabButton.monsterPoolManager = monsterPoolManager;
                prefabButton.equID = equAttackID;
                prefabButton.playerAttack = playerAttack;
                prefabButton.attackID = attackData.attackID;
            }
        }
    }
    public void EquipRefresh(int id)
    {
        equAttackID = id;
        equip = false;
        foreach(var attackData in playerAttack.attackUnlockDatas)
        {
            if(attackData.attackID == equAttackID)
            {
                button.image.sprite = attackData.sprite;
                equip = true;
            }
        }
        if(equip == false)
        {
            button.image.sprite = null;
        }
        playerAttack.AttackRefresh();
    }
}
