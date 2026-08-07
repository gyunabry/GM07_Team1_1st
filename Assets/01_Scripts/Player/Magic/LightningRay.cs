using UnityEngine;
using System.Collections;

public class LightningRay : MonoBehaviour
{
    private ParticleSystem ps;
    [SerializeField] private MonsterPoolManager monsterPoolManager;
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
        yield return new WaitForSeconds(1f);
        monsterPoolManager.ReturnPool(this);
    }
}
