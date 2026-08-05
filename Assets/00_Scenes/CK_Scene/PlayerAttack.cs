using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    IAttack attack;

    [SerializeField] private float distance;
    [SerializeField] private LayerMask layer;
    [SerializeField] private Player player;
    [SerializeField] private MonsterPoolManager poolManager;
    

    public bool isTripleShot = false;
    private void Start()
    {
        attack = new AttackBase();
        if(isTripleShot) GetTripleShot();

        StartCoroutine(AttackCo());
    }
    private void Update()
    {
        
    }
    IEnumerator AttackCo()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            Collider[] enemyIn = Physics.OverlapSphere(transform.position, distance, layer);
            if (enemyIn.Length > 0)
            {
                AttackData ad = new AttackData
                {
                    position = transform.position,
                    direction = (enemyIn[0].transform.position - transform.position).normalized
                };
                attack.Attack(player.attackDamage, ad, poolManager, layer);
            }
        }
    }
    public void GetTripleShot()
    {
        attack = new TripleDeco(attack);
    }
}
