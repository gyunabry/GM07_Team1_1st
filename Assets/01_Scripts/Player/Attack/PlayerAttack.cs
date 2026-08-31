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

    // 1. 공격 코루틴 정리
    // 2. 모든 슬롯을 null
    // 3. slotIndex가 유효 배열 범위인지 검사
    // 4. 실제로 해금된 상태인지 검사
    // 5. 검사를 통과한 스킬만 해당 슬롯에 장착
    public void RestoreEquippedAttacks(IReadOnlyList<EquippedAttackSaveData> savedAttacks)
    {
        AttackPause();

        if (slots == null || slots.Length == 0)
        {
            Debug.LogWarning("복구할 공격 슬롯이 없습니다.");
            return;
        }

        // 모든 슬롯을 null로 초기화
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = new AttackSlotData();
            }

            slots[i].slotIndex = i;
            slots[i].equipAttackID = null;
        }

        if (attackUnlockDatas != null)
        {
            foreach (AttackUnlockData attackData in attackUnlockDatas)
            {
                if (attackData != null)
                {
                    attackData.equip = false;
                }
            }
        }

        if (savedAttacks == null) return;

        foreach (EquippedAttackSaveData savedAttack in savedAttacks)
        {
            if (savedAttack == null || string.IsNullOrWhiteSpace(savedAttack.attackId))
            {
                continue;
            }

            int slotIndex = savedAttack.slotIndex;

            // 저장된 슬롯 인덱스가 정상 범위인지 검사
            if (slotIndex < 0 || slotIndex >= slots.Length)
            {
                Debug.LogWarning($"유효하지 않은 공격 슬롯입니다. 슬롯: {slotIndex}, 공격 ID: {savedAttack.attackId}");
                continue;
            }

            AttackUnlockData matchedAttack = null;

            if (attackUnlockDatas != null)
            {
                foreach (AttackUnlockData attackData in attackUnlockDatas)
                {
                    if (attackData == null) continue;

                    if (string.Equals(attackData.attackID, savedAttack.attackId, System.StringComparison.Ordinal))
                    {
                        matchedAttack = attackData;
                        break;
                    }
                }
            }

            if (matchedAttack == null)
            {
                Debug.LogWarning($"존재하지 않는 공격 ID를 건너뜁니다: {savedAttack.attackId}");
                continue;
            }

            if (!matchedAttack.unlock)
            {
                Debug.LogWarning($"해금되지 않은 공격 스킬은 장착할 수 없습니다: {savedAttack.attackId}");
                continue;
            }

            // 모든 검사를 통과한 공격을 해당 슬롯에 장착
            slots[slotIndex].equipAttackID = savedAttack.attackId;
            matchedAttack.equip = true;
        }
    }

}

[System.Serializable]
public class AttackSlotData
{
    public int slotIndex;
    public string equipAttackID;
}