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
    [SerializeField] private AttackSO MagicArrowSO;
    [SerializeField] private AttackSO FireCircleSO;
    [SerializeField] private AttackSO ChasingSickleSO;
    [SerializeField] private AttackSO LightningRaySO;
    [SerializeField] private AttackSO FlowerThornsSO;

    [SerializeField] public List<AttackUnlockData> attackUnlockDatas = new List<AttackUnlockData>();


    Coroutine[] co;
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
        co = new Coroutine[60];
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
            if (slot.equipAttackID != null)
            {
                string idd = slot.equipAttackID.Replace("S", "");
                if (int.TryParse(idd, out var i)) { }
                if (co[i] != null)
                {
                    StopCoroutine(co[i]);
                }
                co[i] = null;
                if (co[i] == null)
                {
                    switch (i)
                    {
                        case 43:
                            co[i] = StartCoroutine(MagicArrow()); break;
                        case 47:
                            co[i] = StartCoroutine(FireCircle()); break;
                        case 39:
                            co[i] = StartCoroutine(ChasingSickle()); break;
                        case 51:
                            co[i] = StartCoroutine(LightningRay()); break;
                        case 55:
                            co[i] = StartCoroutine(FlowerThorns()); break;
                        default: break;
                    }
                }
            }
        }
        inVillage = false;
    }
    public void StartAndStopAttackCo(int slot, string id,AttackEquHud hud)
    {
        string idd = id.Replace("S", "");
        if(int.TryParse(idd, out var i)) { }
        string nowId = slots[slot].equipAttackID;
        
        if (int.TryParse(nowId, out var j)){ }
        
        bool change = false;
        foreach (var nowSlot in slots)
        {
            if (nowSlot.equipAttackID == id)
            {
                if (nowId == id)
                {
                    slots[slot].equipAttackID = null;
                    if (co[i] != null)
                    {
                        StopCoroutine(co[i]);
                    }
                    co[i] = null;
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
            if(nowId != null)
            {
                if (co[j] != null)
                {
                    StopCoroutine(co[j]);
                    co[j] = null;
                }
            }
            slots[slot].equipAttackID = id;
            if (inVillage == false)
            {
                switch (i)
                {
                    case 43:
                        co[i] = StartCoroutine(MagicArrow()); break;
                    case 47:
                        co[i] = StartCoroutine(FireCircle()); break;
                    case 39:
                        co[i] = StartCoroutine(ChasingSickle()); break;
                    case 51:
                        co[i] = StartCoroutine(LightningRay()); break;
                    case 55:
                        co[i] = StartCoroutine(FlowerThorns()); break;
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
            ad.attackDamage = (player.attackDamage + player.baseAttackDamage) * (MagicArrowSO.attackDamage  + upgrade[1].damage);
            ad.attackSpeed = (MagicArrowSO.attackSpeed + upgrade[1].attackSpeed + player.baseAttackSpeed) + player.attackSpeed;
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
                    AudioManager.Instance.PlaySFX(ESFXType.Active_MagicArrow);
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
            ad.attackDamage = (player.attackDamage + player.baseAttackDamage) * (FireCircleSO.attackDamage + upgrade[2].damage);
            ad.attackSpeed = (FireCircleSO.attackSpeed + upgrade[2].attackSpeed + player.baseAttackDamage) + player.attackSpeed;
            ad.distance = FireCircleSO.distance + upgrade[2].distance + player.baseAttackDistance;

            AudioManager.Instance.PlaySFX(ESFXType.Active_FireCircle);
            attack.FireCircle(ad.attackDamage, ad, poolManager, layer);
            particleManager.GetParticle(1, transform.position, transform.rotation, 0, ad.distance, ad.attackSpeed);

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
            ad.attackDamage = (player.attackDamage + player.baseAttackDamage) * (ChasingSickleSO.attackDamage + upgrade[0].damage);
            ad.attackSpeed = (ChasingSickleSO.attackSpeed + upgrade[0].attackSpeed) + player.attackSpeed + player.baseAttackSpeed;
            ad.distance = ChasingSickleSO.distance + upgrade[0].distance;
            ad.spreadAngle = ChasingSickleSO.spreadAngle;

            AudioManager.Instance.PlaySFX(ESFXType.Active_ChasingSickle);
            attack.ChasingSickle(ad.attackDamage, ad, poolManager, layer);
            particleManager.GetParticle(2, transform.position, transform.rotation, 0, ad.distance, ad.attackSpeed);

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
            ad.attackDamage = (player.attackDamage + player.baseAttackDamage) * (LightningRaySO.attackDamage + upgrade[3].damage);
            ad.attackSpeed = (LightningRaySO.attackSpeed + upgrade[3].attackSpeed) + player.attackSpeed + player.baseAttackSpeed;
            ad.distance = LightningRaySO.distance + upgrade[3].distance + player.baseAttackDistance;
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
                AudioManager.Instance.PlaySFX(ESFXType.Active_LightningRay);
                particleManager.GetParticle(3, transform.position, targetRota, ad.attackDamage, ad.distance, ad.attackSpeed);
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
            ad.attackDamage = (player.attackDamage + player.baseAttackDamage) * (FlowerThornsSO.attackDamage + upgrade[4].damage);
            ad.attackSpeed = (FlowerThornsSO.attackSpeed + upgrade[4].attackSpeed) + player.attackSpeed + player.baseAttackSpeed;
            ad.distance = FlowerThornsSO.distance + upgrade[4].distance + player.baseAttackDistance;
            
            giz = randomPosi;
            dis = ad.distance;
            AudioManager.Instance.PlaySFX(ESFXType.Active_FlowerThorns);
            attack.FlowerThorns(ad.attackDamage, ad, poolManager, layer);
            particleManager.GetParticle(4, randomPosi, transform.rotation, 0, ad.distance, ad.attackSpeed);

            yield return new WaitForSeconds(ad.attackSpeed);
        }
    }
}
[System.Serializable]
public class AttackSlotData
{
    public int slotIndex;
    public string equipAttackID;
}