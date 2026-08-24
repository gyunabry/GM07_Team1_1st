using System.Collections;
using UnityEngine;

public class ChasingSickle : MonoBehaviour
{
    private ParticleSystem ps;
    [SerializeField] private PoolManager poolManager;
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
        poolManager.ReturnPool(this);
    }
}
