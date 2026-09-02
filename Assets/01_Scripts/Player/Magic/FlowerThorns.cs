using UnityEngine;
using System.Collections;

public class FlowerThorns : MonoBehaviour
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
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        monsterPoolManager.ReturnPool(this);
    }
}
