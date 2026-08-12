using UnityEngine;

public class AttackEquPrefab : MonoBehaviour
{
    public PlayerAttack playerAttack;
    public AttackEquHud attackEquHud;
    public MonsterPoolManager monsterPoolManager;
    public int attackID;
    public int equID;

    private void OnDisable()
    {
        monsterPoolManager.ReturnPool(this);
    }
    public void EquipSkill()
    {
        foreach (var attack in playerAttack.attackUnlockDatas)
        {
            if (attack.attackID == attackID)
            {
                if(equID == attackID)
                {
                    attack.equip = false;
                }
                else if(attack.equip == true)
                {
                    Debug.Log("이미 장착중인 공격마법입니다.");
                }
                else
                {
                    attack.equip = true;
                }
            }
           
        }
    }
}
