using UnityEngine;
using System.Collections;

public class FireCircle : MonoBehaviour
{
    private ParticleSystem ps;
    [SerializeField] private PoolManager monsterPoolManager;
    [SerializeField] private AttackSO attackSo;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }
    private void OnEnable()
    {
        StartCoroutine(PlayCo());
    }
    IEnumerator PlayCo()
    {
        yield return new WaitForSeconds(attackSo.attackSpeed);
        monsterPoolManager.ReturnPool(this);
    }
}
