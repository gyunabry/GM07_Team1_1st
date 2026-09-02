using UnityEngine;
using System.Collections;

public class LightningRay : MonoBehaviour
{
    private ParticleSystem ps;
    [SerializeField] private PoolManager poolManager;
    [SerializeField] private AttackSO attackSo;
    public float damage;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }
    private void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * 30f, Space.Self);
    }
    private void OnEnable()
    {
        StartCoroutine(PlayCo());
    }
    IEnumerator PlayCo()
    {
        yield return new WaitForSeconds(0.5f);
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        poolManager.ReturnPool(this);
    }
    private void OnTriggerEnter(Collider other)
    {
        Enemy ene = other.gameObject.GetComponent<Enemy>();
        if (ene != null)
        {
            AudioManager.Instance.PlaySFX(ESFXType.Hit_LightningRay);
            ene.TakeDamage(damage);
        }
    }
    
}
