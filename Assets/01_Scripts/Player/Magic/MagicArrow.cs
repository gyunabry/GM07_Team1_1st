using UnityEngine;
using System.Collections;

public class MagicArrow : MonoBehaviour
{
    private ParticleSystem ps;
    [SerializeField] private PoolManager poolManager;

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
        if (ps.isPlaying == true)
        {
            yield return null;
        }
        else
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            poolManager.ReturnPool(this);
        }
    }
}
