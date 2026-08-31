using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    IAttack attack;

    [SerializeField] private LayerMask layer;
    [SerializeField] private Player player;
    [SerializeField] private PoolManager poolManager;
    [SerializeField] private ParticleManager particleManager;

    [SerializeField] private List<AttackSO> attackSOData;

    [SerializeField] public List<AttackUnlockData> attackUnlockDatas = new List<AttackUnlockData>();

    Dictionary<string, AttackSO> attackDictionary = new Dictionary<string, AttackSO>();
    Dictionary<string, Coroutine> attackCoDictionary = new Dictionary<string, Coroutine>();
    public AttackSlotData[] slots = new AttackSlotData[60];
    public PlayerAttackUpgrade[] upgrade = new PlayerAttackUpgrade[60];

    Vector3 giz;
    float dis;
    private bool inVillage = true;
    private void Awake()
    {
        for (int i = 0; i < slots.Length; i++) 
        { 
            slots[i] = new AttackSlotData();
            slots[i].slotIndex = i;
            slots[i].equipAttackID = null;
        }
        for (int i = 0; i < upgrade.Length; i++) 
        {
            upgrade[i] = new PlayerAttackUpgrade();
            upgrade[i].damage = 0f;
            upgrade[i].attackSpeed = 0f;
            upgrade[i].distance = 0f;
            upgrade[i].projectileCount = 0;
        }
        attackUnlockDatas.Clear();
        foreach(var attackData in attackSOData)
        {
            AttackUnlockData aud = new AttackUnlockData();
            aud.attackID = attackData.attackID;
            aud.unlock = false;
            aud.equip = false;
            if(attackData.sprite != null)
            {
                aud.sprite = attackData.sprite;
            }
            attackUnlockDatas.Add(aud);
        }
        attackDictionary.Clear();
        foreach(var attackData in attackSOData)
        {
            if(attackData != null && !attackDictionary.ContainsKey(attackData.attackID))
            {
                attackDictionary.Add(attackData.attackID, attackData);
            }
        }
    }
    private void Start()
    {
        attack = new AttackBase();
    }
    public void AttackPause()
    {
        int i = 0;
        foreach(var cor in attackCoDictionary.Values)
        {
            if(cor != null)
            {
                StopCoroutine(cor);
            }
        }
        attackCoDictionary.Clear();
        inVillage = true;
    }
    private int GetUpgradeIndex(string skillID)
    {
        return skillID switch
        {
            "S39" => 0,
            "S43" => 1,
            "S47" => 2,
            "S51" => 3,
            "S55" => 4,
            _ => 0
        };
    }
    public void AttackRefresh()
    {
        foreach (var slot in slots)
        {
            if (!string.IsNullOrEmpty(slot.equipAttackID))
            {
                string skillID = slot.equipAttackID;
                if (attackDictionary.TryGetValue(skillID, out AttackSO attackSO))
                {
                    if(attackCoDictionary.TryGetValue(skillID, out Coroutine runningCo))
                    {
                        if(runningCo != null) StopCoroutine(runningCo);
                        attackCoDictionary.Remove(skillID);
                    }
                    int upgradeIndex = GetUpgradeIndex(skillID);
                    attackCoDictionary[skillID] = StartCoroutine(SkillCoroutine(attackSO, upgradeIndex));
                }
            }
        }
        inVillage = false;
    }
    public void StartAndStopAttackCo(int slot, string id,AttackEquHud hud)
    {
        string nowId = slots[slot].equipAttackID;
        bool change = false;
        
        foreach (var nowSlot in slots)
        {
            if (nowSlot.equipAttackID == id)
            {
                if (nowId == id)
                {
                    slots[slot].equipAttackID = null;
                    if(attackCoDictionary.TryGetValue(id, out Coroutine runningCo))
                    {
                        if (runningCo != null) StopCoroutine(runningCo);
                        attackCoDictionary.Remove(id);
                    }
                    hud.EquipRefresh(id);
                }
                else
                {
                    Debug.Log("다른 슬롯에 장착되어 있습니다.");
                }
                change = true;
            }
        }
        if(!change)
        {
            if(nowId != null && attackCoDictionary.TryGetValue(nowId, out Coroutine oldCo))
            {
                if (oldCo != null) StopCoroutine(oldCo);
                attackCoDictionary.Remove(nowId);
            }
            slots[slot].equipAttackID = id;
            if (inVillage == false)
            {
                if (attackDictionary.TryGetValue(id, out AttackSO attackSO))
                {
                    int upgradeIndex = GetUpgradeIndex(id);
                    attackCoDictionary[id] = StartCoroutine(SkillCoroutine(attackSO, upgradeIndex));
                }
            }
            hud.EquipRefresh(id);
        }
    }
    IEnumerator SkillCoroutine(AttackSO attackSO, int upgradeIndex)
    {
        while (true)
        {
            AttackData ad = attackSO.CalculateAttackData(player, upgrade[upgradeIndex]);
            
            if(attackSO.skillBase != null)
            {
                yield return StartCoroutine(attackSO.skillBase.RunSkill(player, ad, poolManager, particleManager,layer, attack));
            }
            yield return new WaitForSeconds(ad.attackSpeed);
        }
    }
    public float[] ReturnSkillValue(string skillCode)
    {
        float[] value = new float[4];
        if(attackDictionary.TryGetValue(skillCode, out AttackSO attackSO))
        {
            string code = skillCode.Replace("S", "");
            int.TryParse(code, out int upgradeIndex);

            AttackData ad = attackSO.CalculateAttackData(player, upgrade[upgradeIndex]);

            value[0] = ad.attackDamage;
            value[1] = ad.attackSpeed;
            value[2] = ad.distance;
            value[3] = ad.projectileCount;
        }
        return value;
    }
}
[System.Serializable]
public class AttackSlotData
{
    public int slotIndex;
    public string equipAttackID;
}