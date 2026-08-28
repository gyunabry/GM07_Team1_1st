using UnityEngine;
using System.Collections;

public class FireCircle : MonoBehaviour
{
    private ParticleSystem ps;
    // [SerializeField] private PoolManager poolManager;
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
        PoolManager.Instance.ReturnPool(this);
    }
}