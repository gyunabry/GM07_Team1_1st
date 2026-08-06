using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class AttackPoint : MonoBehaviour
{
    public int attackDamage;
    public MonsterPoolManager poolManager;
    public LayerMask layer;

    private void OnEnable()
    {
        StartCoroutine(ReturnCo());
    }
    private void FixedUpdate()
    {
        transform.Translate(transform.forward * Time.deltaTime * 10f, Space.World);
    }
    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("!");
        if ((layer & (1 << other.gameObject.layer)) == 0) return;
        Debug.Log("?");
        Enemy enemy = other.gameObject.GetComponent<Enemy>();
        enemy.TakeDamage(attackDamage);
        poolManager.ReturnPool(this);
    }
    IEnumerator ReturnCo()
    {
        yield return new WaitForSeconds(5f);
        poolManager.ReturnPool(this);
    }
}
