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
        yield return new WaitForSeconds(1.2f);
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        poolManager.ReturnPool(this);
    }
}
