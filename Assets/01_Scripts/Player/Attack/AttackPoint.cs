using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class AttackPoint : MonoBehaviour
{
    public float attackDamage;
    public PoolManager poolManager;
    public LayerMask layer;
    public Enemy enemy;


    private void OnEnable()
    {
        StartCoroutine(ReturnCo());
    }
    private void FixedUpdate()
    {
        if(enemy != null)
        {
            if(!(enemy.stateController.nowState == enemy.stateController.DieState))
            {
                transform.LookAt(enemy.transform.position);
            }
        }
            transform.Translate(transform.forward * Time.deltaTime * 10f, Space.World);
    }
    private void OnCollisionEnter(Collision other)
    {
        if ((layer & (1 << other.gameObject.layer)) == 0) return;
        Enemy enemy = other.gameObject.GetComponent<Enemy>();
        AudioManager.Instance.PlaySFX(ESFXType.Hit_MagicArrow);
        enemy.TakeDamage(attackDamage);
        poolManager.ReturnPool(this);
    }
    IEnumerator ReturnCo()
    {
        yield return new WaitForSeconds(5f);
        poolManager.ReturnPool(this);
    }
}
