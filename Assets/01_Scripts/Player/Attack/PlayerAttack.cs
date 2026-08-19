using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    IAttack attack;

    [SerializeField] private LayerMask layer;
    [SerializeField] private Player player;
    [SerializeField] private MonsterPoolManager poolManager;
    [SerializeField] private ParticleManager particleManager;

    [SerializeField] private List<AttackSO> attackSOData;
    [SerializeField] private AttackSO MagicArrowSO;
    [SerializeField] private AttackSO FireCircleSO;
    [SerializeField] private AttackSO ChasingSickleSO;
    [SerializeField] private AttackSO LightningRaySO;
    [SerializeField] private AttackSO FlowerThornsSO;

    [SerializeField] public List<AttackUnlockData> attackUnlockDatas = new List<AttackUnlockData>();


    Coroutine[] co;
    public AttackSlotData[] slots = new AttackSlotData[6];
    public PlayerAttackUpgrade[] upgrade = new PlayerAttackUpgrade[6];

    Vector3 giz;
    float dis;
    private bool inVillage = true;
    private void Awake()
    {
        for (int i = 0; i < slots.Length; i++) 
        { 
            slots[i] = new AttackSlotData();
            slots[i].slotIndex = i;
            slots[i].equipAttackID = -1;
        }
        
        co = new Coroutine[20];
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
        
    }
    private void Start()
    {
        attack = new AttackBase();
    }
    public void AttackPause()
    {
        int i = 0;
        foreach(var cor in co)
        {
            if(cor != null)
            {
                StopCoroutine(cor);
            }
            co[i] = null;
            i++;
        }
        inVillage = true;
    }
    public void AttackRefresh()
    {
        foreach (var slot in slots)
        {
            if (slot.equipAttackID != -1)
            {
                if (co[slot.equipAttackID] != null)
                {
                    StopCoroutine(co[slot.equipAttackID]);
                }
                co[slot.equipAttackID] = null;
                if (co[slot.equipAttackID] == null)
                {
                    switch (slot.equipAttackID)
                    {
                        case 1:
                            co[slot.equipAttackID] = StartCoroutine(MagicArrow()); break;
                        case 2:
                            co[slot.equipAttackID] = StartCoroutine(FireCircle()); break;
                        case 3:
                            co[slot.equipAttackID] = StartCoroutine(ChasingSickle()); break;
                        case 4:
                            co[slot.equipAttackID] = StartCoroutine(LightningRay()); break;
                        case 5:
                            co[slot.equipAttackID] = StartCoroutine(FlowerThorns()); break;
                        default: break;
                    }
                }
            }
        }
        inVillage = false;
    }
    public void StartAndStopAttackCo(int slot, int id,AttackEquHud hud)
    {
        int nowId = slots[slot].equipAttackID;
        bool change = false;
        foreach (var nowSlot in slots)
        {
            if (nowSlot.equipAttackID == id)
            {
                if (nowId == id)
                {
                    slots[slot].equipAttackID = -1;
                    if (co[id] != null)
                    {
                        StopCoroutine(co[id]);
                    }
                    co[id] = null;
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
            if(nowId != -1)
            {
                if (co[nowId] != null)
                {
                    StopCoroutine(co[nowId]);
                    co[nowId] = null;
                }
            }
            slots[slot].equipAttackID = id;
            if (inVillage == false)
            {
                switch (id)
                {
                    case 1:
                        co[id] = StartCoroutine(MagicArrow()); break;
                    case 2:
                        co[id] = StartCoroutine(FireCircle()); break;
                    case 3:
                        co[id] = StartCoroutine(ChasingSickle()); break;
                    case 4:
                        co[id] = StartCoroutine(LightningRay()); break;
                    case 5:
                        co[id] = StartCoroutine(FlowerThorns()); break;
                    default: break;
                }
            }
            hud.EquipRefresh(id);
        }
    }
    IEnumerator MagicArrow() // 코드 1
    {
        Collider[] enemyIn;
        
        while (true)
        {
            AttackData ad = new AttackData
            {
                position = transform.position,
            };
            ad.distance = MagicArrowSO.distance;
            enemyIn = Physics.OverlapSphere(transform.position, ad.distance, layer);
            ad.attackDamage = (MagicArrowSO.attackDamage + upgrade[1].damage) * player.AttackDamage;
            ad.attackSpeed = (MagicArrowSO.attackSpeed + upgrade[1].attackSpeed) + player.AttackSpeed;
            ad.projectileCount = MagicArrowSO.projectileCount + upgrade[1].projectileCount;
            if(enemyIn == null)
            {
                yield return null;
                continue;
            }
            if (enemyIn.Length == 0)
            {
                yield return null;
                continue;
            }
            ad.direction = (enemyIn[0].transform.position - transform.position).normalized;
            for (int i = 0; i < ad.projectileCount; i++)
            {
                if (enemyIn.Length == 0)
                {
                    enemyIn = Physics.OverlapSphere(transform.position, ad.distance, layer);
                }
                if (enemyIn.Length > 0)
                {
                    attack.MagicArrow(ad.attackDamage, ad, poolManager, layer);
                    if(i == ad.projectileCount - 1)
                    {
                        break;
                    }
                    yield return new WaitForSeconds(0.1f);
                }
            }
            yield return new WaitForSeconds(ad.attackSpeed);
        }
    }
    IEnumerator FireCircle()// 코드 2
    {
        while (true)
        {
            AttackData ad = new AttackData
            {
                position = transform.position,
            };
            ad.attackDamage = (FireCircleSO.attackDamage + upgrade[2].damage) * player.AttackDamage;
            ad.attackSpeed = (FireCircleSO.attackSpeed + upgrade[2].attackSpeed) + player.AttackSpeed;
            ad.distance = FireCircleSO.distance + upgrade[2].distance;
            
            attack.FireCircle(ad.attackDamage, ad, poolManager, layer);
            particleManager.GetParticle(1, transform.position, transform.rotation, 0);

            yield return new WaitForSeconds(ad.attackSpeed);
        }
    }
    IEnumerator ChasingSickle()// 코드 3
    {
        while (true)
        {
            AttackData ad = new AttackData
            {
                position = transform.position,
                forward = transform.forward
            };
            ad.attackDamage = (ChasingSickleSO.attackDamage + upgrade[3].damage) * player.AttackDamage;
            ad.attackSpeed = (ChasingSickleSO.attackSpeed + upgrade[3].attackSpeed) + player.AttackSpeed;
            ad.distance = ChasingSickleSO.distance + upgrade[3].distance;
            ad.spreadAngle = ChasingSickleSO.spreadAngle;

            attack.ChasingSickle(ad.attackDamage, ad, poolManager, layer);
            particleManager.GetParticle(2, transform.position, transform.rotation, 0);

            yield return new WaitForSeconds(ad.attackSpeed);
        }
    }
    IEnumerator LightningRay()// 코드 4
    {
        while (true)
        {
            AttackData ad = new AttackData
            {
                position = transform.position,
            };
            ad.attackDamage = (LightningRaySO.attackDamage + upgrade[4].damage) * player.AttackDamage;
            ad.attackSpeed = (LightningRaySO.attackSpeed + upgrade[4].attackSpeed) + player.AttackSpeed;
            ad.distance = LightningRaySO.distance + upgrade[4].distance;
            ad.spreadAngle = LightningRaySO.spreadAngle;

            Collider[] enemy = Physics.OverlapSphere(transform.position, ad.distance, layer);

            Collider nearEnemy = null;
            float minDis = Mathf.Infinity;
            if (enemy.Length > 0)
            {
                foreach (var that in enemy)
                {
                    Vector3 enemyPosi = that.transform.position;
                    float distance = (transform.position - enemyPosi).sqrMagnitude;
                    if (distance < minDis)
                    {
                        minDis = distance;
                        nearEnemy = that;
                    }
                }   
            Vector3 dir = (nearEnemy.transform.position - transform.position).normalized;
            Quaternion targetRota = Quaternion.LookRotation(dir);
            particleManager.GetParticle(3, transform.position, targetRota, ad.attackDamage);
            }
            else
            {
                yield return null;
                continue;
            }
            yield return new WaitForSeconds(ad.attackSpeed);
        }
    }
    IEnumerator FlowerThorns()// 코드 5
    {
        while (true)
        {
            Vector2 randomCircle = Random.insideUnitCircle * FlowerThornsSO.distance;
            Vector3 randomPosi = new Vector3(transform.position.x + randomCircle.x, transform.position.y, transform.position.z + randomCircle.y);
            AttackData ad = new AttackData
            {
                position = randomPosi,
            };
            ad.attackDamage = (FlowerThornsSO.attackDamage + upgrade[5].damage) * player.AttackDamage;
            ad.attackSpeed = (FlowerThornsSO.attackSpeed + upgrade[5].attackSpeed) + player.AttackSpeed;
            ad.distance = FlowerThornsSO.distance + upgrade[5].distance;
            
            giz = randomPosi;
            dis = ad.distance;
            attack.FlowerThorns(ad.attackDamage, ad, poolManager, layer);
            particleManager.GetParticle(4, randomPosi, transform.rotation, 0);

            yield return new WaitForSeconds(ad.attackSpeed);
        }
    }
}
[System.Serializable]
public class AttackSlotData
{
    public int slotIndex;
    public int equipAttackID;
}