using System.Collections;
using System.Collections.Generic;
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
    private List<IEnumerator> coroutineDelegate = new List<IEnumerator>();

    public bool isTripleShot = false;
    public bool isMagicArrow = false;
    public bool isFireCircle = false;
    public bool isChasingSickle = false;
    public bool isLightningRay = false;
    public bool isFlowerThorns = false;

    Coroutine[] co;
    

    Vector3 giz;
    float dis;
    private void Awake()
    {
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
        coroutineDelegate.Add(MagicArrow());
        coroutineDelegate.Add(FireCircle());
        coroutineDelegate.Add(ChasingSickle());
        coroutineDelegate.Add(LightningRay());
        coroutineDelegate.Add(FlowerThorns());
    }
    private void Start()
    {
        attack = new AttackBase();
        if(isTripleShot) GetTripleShot();
        AttackRefresh();
    }
    
    public void AttackRefresh()
    {
        int i = 0;
        foreach(var equip in attackUnlockDatas)
        {
            if(equip.equip == true)
            {
                if (co[i] != null) continue;
                co[i] = StartCoroutine(coroutineDelegate[i]);
            }
            else if(equip.equip == false)
            {
                if (co[i] != null)
                {
                    StopCoroutine(co[i]);
                    co[i] = null;
                }
            }
            i++;
        }
    }
    public void StartAndStopAttackCo(int id)
    {
        switch(id){
            case 1:
                if (co != null)
                {
                    co[1] = StartCoroutine(MagicArrow());
                }
                else
                {
                    StopCoroutine(co[1]);
                }
                    break;
            case 2:
                if (co != null)
                {
                    co[2] = StartCoroutine(FireCircle());
                }
                else
                {
                    StopCoroutine(co[2]);
                }
                break;
            case 3:
                if (co != null)
                {
                    co[3] = StartCoroutine(ChasingSickle());
                }
                else
                {
                    StopCoroutine(co[3]);
                }
                break;
            case 4:
                if (co != null)
                {
                    co[4] = StartCoroutine(LightningRay());
                }
                else
                {
                    StopCoroutine(co[4]);
                }
                break;
            case 5:
                if (co != null)
                {
                    co[5] = StartCoroutine(FlowerThorns());
                }
                else
                {
                    StopCoroutine(co[5]);
                }
                break;
            default: break;

        }
    }
    IEnumerator MagicArrow() // 内靛 1
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
            ad.attackDamage = MagicArrowSO.attackDamage + player.AttackDamage;
            ad.attackSpeed = MagicArrowSO.attackSpeed + player.AttackSpeed;
            ad.projectileCount = MagicArrowSO.projectileCount;
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
    IEnumerator FireCircle()// 内靛 2
    {
        while (true)
        {
            AttackData ad = new AttackData
            {
                position = transform.position,
            };
            ad.attackDamage = FireCircleSO.attackDamage + player.AttackDamage;
            ad.attackSpeed = FireCircleSO.attackSpeed + player.AttackSpeed;
            ad.distance = FireCircleSO.distance;
            
            attack.FireCircle(ad.attackDamage, ad, poolManager, layer);
            particleManager.GetParticle(1, transform.position, transform.rotation);

            yield return new WaitForSeconds(ad.attackSpeed);
        }
    }
    IEnumerator ChasingSickle()// 内靛 3
    {
        while (true)
        {
            AttackData ad = new AttackData
            {
                position = transform.position,
                forward = transform.forward
            };
            ad.attackDamage = ChasingSickleSO.attackDamage + player.AttackDamage;
            ad.attackSpeed = ChasingSickleSO.attackSpeed + player.AttackSpeed;
            ad.distance = ChasingSickleSO.distance;
            ad.spreadAngle = ChasingSickleSO.spreadAngle;

            attack.ChasingSickle(ad.attackDamage, ad, poolManager, layer);
            particleManager.GetParticle(0, transform.position, transform.rotation);

            yield return new WaitForSeconds(ad.attackSpeed);
        }
    }
    IEnumerator LightningRay()// 内靛 4
    {
        while (true)
        {
            AttackData ad = new AttackData
            {
                position = transform.position,
            };
            ad.attackDamage = LightningRaySO.attackDamage + player.AttackDamage;
            ad.attackSpeed = LightningRaySO.attackSpeed + player.AttackSpeed;
            ad.distance = LightningRaySO.distance;
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
            attack.LightningRay(ad.attackDamage, ad, poolManager, layer);
            particleManager.GetParticle(3, transform.position, targetRota);
            }
            else
            {
                yield return null;
                continue;
            }
            yield return new WaitForSeconds(ad.attackSpeed);
        }
    }
    IEnumerator FlowerThorns()// 内靛 5
    {
        while (true)
        {
            Vector2 randomCircle = Random.insideUnitCircle * FlowerThornsSO.distance;
            Vector3 randomPosi = new Vector3(transform.position.x + randomCircle.x, transform.position.y, transform.position.z + randomCircle.y);
            AttackData ad = new AttackData
            {
                position = randomPosi,
            };
            ad.attackDamage = FlowerThornsSO.attackDamage + player.AttackDamage;
            ad.attackSpeed = FlowerThornsSO.attackSpeed + player.AttackSpeed;
            ad.distance = FlowerThornsSO.distance;
            
            giz = randomPosi;
            dis = ad.distance;
            attack.FlowerThorns(ad.attackDamage, ad, poolManager, layer);
            particleManager.GetParticle(4, randomPosi, transform.rotation);

            yield return new WaitForSeconds(ad.attackSpeed);
        }
    }
    public void OnOffMagicArrow()
    {
        isMagicArrow = !isMagicArrow;
    }
    public void OnOffFireCircle()
    {
        isFireCircle = !isFireCircle;
    }
    public void OnOffChasingSickle()
    {
        isChasingSickle = !isChasingSickle;
    }
    public void OnOffFlowerThorns()
    {
        isFlowerThorns = !isFlowerThorns;
    }
    public void OnOffLightningRay()
    {
        isLightningRay = !isLightningRay;
    }
    public void GetTripleShot()

    {
        attack = new TripleDeco(attack);
    }
}
