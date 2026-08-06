using System.Collections;
using UnityEngine;

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
    

    public bool isTripleShot = false;
    public bool isAttack = false;
    public bool isSkill = false;
    public bool isSkill2 = false;
    public bool isSkill3 = false;
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
        if (isAttack) StartCoroutine(AttackCo());
        if (isSkill) StartCoroutine(SkillCo());
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
            if (enemyIn.Length > 0) continue;
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

            Collider[] enemyIn = Physics.OverlapSphere(transform.position, ad.distance, layer);
            if (enemyIn.Length > 0)
            {
                attack.Skill(ad.attackDamage, ad, poolManager, layer);
            }
            yield return new WaitForSeconds(ad.attackSpeed);
        }
    }
    public void GetTripleShot()
    {
        attack = new TripleDeco(attack);
    }
}
