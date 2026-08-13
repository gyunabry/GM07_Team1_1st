using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerAttack : MonoBehaviour
{
    IAttack attack;

    [SerializeField] private float distance;
    [SerializeField] private LayerMask layer;
    [SerializeField] private Player player;
    [SerializeField] private MonsterPoolManager poolManager;
    [SerializeField] private ParticleManager particleManager;

    [SerializeField] private AttackSO MagicArrowSO;
    [SerializeField] private AttackSO FireCircleSO;
    [SerializeField] private AttackSO ChasingSickleSO;
    [SerializeField] private AttackSO LightningRaySO;
    [SerializeField] private AttackSO FlowerThornsSO;

    public bool isTripleShot = false;
    public bool isMagicArrow = false;
    public bool isFireCircle = false;
    public bool isChasingSickle = false;
    public bool isLightningRay = false;
    public bool isFlowerThorns = false;

    Coroutine co;

    Vector3 giz;
    float dis;
    private void Start()
    {
        attack = new AttackBase();
        if(isTripleShot) GetTripleShot();
        AllAttackStart();
    }
    private void Update()
    {
        
    }
    public void AllAttackStart()
    {
        if(co != null) StopCoroutine(co);
        co = null;
        if (isMagicArrow) co = StartCoroutine(MagicArrow());
        if (isFireCircle) co = StartCoroutine(FireCircle());
        if (isChasingSickle) co = StartCoroutine(ChasingSickle());
        if (isLightningRay) co = StartCoroutine(LightningRay());
        if (isFlowerThorns) co = StartCoroutine(FlowerThorns());
        
    }
    IEnumerator MagicArrow()
    {
        while (true)
        {
            AttackData ad = new AttackData
            {
                position = transform.position,
            };
            Collider[] enemyIn = Physics.OverlapSphere(transform.position, distance, layer);
            ad.attackDamage = MagicArrowSO.attackDamage + player.AttackDamage;
            ad.attackSpeed = MagicArrowSO.attackSpeed + player.AttackSpeed;
            if (enemyIn.Length == 0)
            {
                yield return null;
                continue;
            }
            ad.direction = (enemyIn[0].transform.position - transform.position).normalized;
            ad.spreadAngle = MagicArrowSO.spreadAngle;

            if (enemyIn.Length > 0)
            {
                attack.MagicArrow(ad.attackDamage, ad, poolManager, layer);
            }
            yield return new WaitForSeconds(ad.attackSpeed);
        }
    }
    IEnumerator FireCircle()
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
    IEnumerator ChasingSickle()
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
    IEnumerator LightningRay()
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
    IEnumerator FlowerThorns()
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
    public void GetTripleShot()

    {
        attack = new TripleDeco(attack);
    }
}
