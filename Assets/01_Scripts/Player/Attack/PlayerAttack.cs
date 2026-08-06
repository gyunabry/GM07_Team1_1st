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

    [SerializeField] private AttackSO attackSO;
    [SerializeField] private AttackSO skillSO;
    [SerializeField] private AttackSO skill2SO;
    [SerializeField] private AttackSO skill3SO;
    [SerializeField] private AttackSO skill4SO;
    

    public bool isTripleShot = false;
    public bool isAttack = false;
    public bool isSkill = false;
    public bool isSkill2 = false;
    public bool isSkill3 = false;
    public bool isSkill4 = false;

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
        StopCoroutine(AttackCo());
        StopCoroutine(SkillCo());
        StopCoroutine(Skill2Co());
        StopCoroutine(Skill3Co());
        StopCoroutine(Skill4Co());
        if (isAttack) StartCoroutine(AttackCo());
        if (isSkill) StartCoroutine(SkillCo());
        if (isSkill2) StartCoroutine(Skill2Co());
        if (isSkill3) StartCoroutine(Skill3Co());
        if (isSkill4) StartCoroutine(Skill4Co());
    }
    IEnumerator AttackCo()
    {
        while (true)
        {
            AttackData ad = new AttackData
            {
                position = transform.position,
            };
            Collider[] enemyIn = Physics.OverlapSphere(transform.position, distance, layer);
            ad.attackDamage = attackSO.attackDamage;
            ad.attackSpeed = attackSO.attackSpeed;
            if (enemyIn.Length > 0)
            {
                yield return null;
                continue;
            }
            ad.direction = (enemyIn[0].transform.position - transform.position).normalized;
            ad.spreadAngle = attackSO.spreadAngle;

            if (enemyIn.Length > 0)
            {
                attack.Attack(ad.attackDamage, ad, poolManager, layer);
            }
            yield return new WaitForSeconds(ad.attackSpeed);
        }
    }
    IEnumerator SkillCo()
    {
        while (true)
        {
            AttackData ad = new AttackData
            {
                position = transform.position,
            };
            ad.attackDamage = skillSO.attackDamage;
            ad.attackSpeed = skillSO.attackSpeed;
            ad.distance = skillSO.distance;
            
            attack.Skill(ad.attackDamage, ad, poolManager, layer);
            
            yield return new WaitForSeconds(ad.attackSpeed);
        }
    }
    IEnumerator Skill2Co()
    {
        while (true)
        {
            AttackData ad = new AttackData
            {
                position = transform.position,
                forward = transform.forward
            };
            ad.attackDamage = skill2SO.attackDamage;
            ad.attackSpeed = skill2SO.attackSpeed;
            ad.distance = skill2SO.distance;
            ad.spreadAngle = skill2SO.spreadAngle;

            attack.Skill2(ad.attackDamage, ad, poolManager, layer);

            yield return new WaitForSeconds(ad.attackSpeed);
        }
    }
    IEnumerator Skill3Co()
    {
        while (true)
        {
            AttackData ad = new AttackData
            {
                position = transform.position,
            };
            ad.attackDamage = skill3SO.attackDamage;
            ad.attackSpeed = skill3SO.attackSpeed;
            ad.distance = skill3SO.distance;
            ad.spreadAngle = skill3SO.spreadAngle;

            attack.Skill3(ad.attackDamage, ad, poolManager, layer);

            yield return new WaitForSeconds(ad.attackSpeed);
        }
    }
    IEnumerator Skill4Co()
    {
        while (true)
        {
            Vector2 randomCircle = Random.insideUnitCircle * skill4SO.distance;
            Vector3 randomPosi = new Vector3(transform.position.x + randomCircle.x, transform.position.y, transform.position.z + randomCircle.y);
            AttackData ad = new AttackData
            {
                position = randomPosi,
            };
            ad.attackDamage = skill4SO.attackDamage;
            ad.attackSpeed = skill4SO.attackSpeed;
            ad.distance = skill4SO.distance;
            
            
            giz = randomPosi;
            dis = ad.distance;
            attack.Skill4(ad.attackDamage, ad, poolManager, layer);

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
