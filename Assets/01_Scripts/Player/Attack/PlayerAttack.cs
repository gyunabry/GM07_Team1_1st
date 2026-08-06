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
        StopCoroutine(MagicArrow());
        StopCoroutine(FireCircle());
        StopCoroutine(ChasingSickle());
        StopCoroutine(LightningRay());
        StopCoroutine(FlowerThorns());
        if (isMagicArrow) StartCoroutine(MagicArrow());
        if (isFireCircle) StartCoroutine(FireCircle());
        if (isChasingSickle) StartCoroutine(ChasingSickle());
        if (isLightningRay) StartCoroutine(LightningRay());
        if (isFlowerThorns) StartCoroutine(FlowerThorns());
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
            ad.attackDamage = MagicArrowSO.attackDamage;
            ad.attackSpeed = MagicArrowSO.attackSpeed;
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
            ad.attackDamage = FireCircleSO.attackDamage;
            ad.attackSpeed = FireCircleSO.attackSpeed;
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
            ad.attackDamage = ChasingSickleSO.attackDamage;
            ad.attackSpeed = ChasingSickleSO.attackSpeed;
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
            ad.attackDamage = LightningRaySO.attackDamage;
            ad.attackSpeed = LightningRaySO.attackSpeed;
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
            }
            float dis = minDis - transform.position.sqrMagnitude;
            attack.LightningRay(ad.attackDamage, ad, poolManager, layer);
            particleManager.GetParticle(3, transform.position, new Quaternion(0f, dis, 0f, 0f));

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
            ad.attackDamage = FlowerThornsSO.attackDamage;
            ad.attackSpeed = FlowerThornsSO.attackSpeed;
            ad.distance = FlowerThornsSO.distance;
            
            giz = randomPosi;
            dis = ad.distance;
            attack.FlowerThorns(ad.attackDamage, ad, poolManager, layer);
            particleManager.GetParticle(4, randomPosi, transform.rotation);

            yield return new WaitForSeconds(ad.attackSpeed);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(giz, dis);
    }
    public void GetTripleShot()
    {
        attack = new TripleDeco(attack);
    }
}
