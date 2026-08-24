using System.Collections;
using UnityEngine;

public class ParticleMain : MonoBehaviour
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
        if(ps.isPlaying == true)
        {
            yield return null;
        }
        else
        {
            poolManager.ReturnPool(this);
        }
    }
}
