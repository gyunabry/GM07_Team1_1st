using UnityEngine;

/// <summary>
/// 손님의 결제가 완료되면 손님 위치에서 보상 이펙트를 재생합니다.
/// </summary>
[RequireComponent(typeof(CustomerController))]
[DisallowMultipleComponent]
public sealed class CustomerPaymentVfx : MonoBehaviour
{
    [SerializeField] private GameObject paymentCompletedEffectPrefab;
    [SerializeField] private Transform vfxSpawnPoint;

    private CustomerController customer;
    private GameObject effectObject;
    private ParticleSystem[] particleSystems;

    private void Awake()
    {
        customer = GetComponent<CustomerController>();
        CreateEffect();
    }

    private void OnEnable()
    {
        if (customer != null)
        {
            customer.PaymentCompleted += PlayPaymentCompletedEffect;
        }
    }

    private void OnDisable()
    {
        if (customer != null)
        {
            customer.PaymentCompleted -= PlayPaymentCompletedEffect;
        }

        StopEffect();
    }

    private void OnDestroy()
    {
        if (effectObject != null)
        {
            Destroy(effectObject);
        }
    }

    private void CreateEffect()
    {
        if (paymentCompletedEffectPrefab == null)
        {
            Debug.LogWarning("CustomerPaymentVfx requires a payment completed effect prefab.", this);
            return;
        }

        effectObject = Instantiate(paymentCompletedEffectPrefab, transform);
        effectObject.SetActive(false);
        particleSystems = effectObject.GetComponentsInChildren<ParticleSystem>(true);
    }

    private void PlayPaymentCompletedEffect()
    {
        if (effectObject == null)
        {
            return;
        }

        Transform spawnPoint = vfxSpawnPoint != null ? vfxSpawnPoint : transform;
        effectObject.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        effectObject.SetActive(true);

        for (int i = 0; i < particleSystems.Length; i++)
        {
            particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystems[i].Play(true);
        }
    }

    private void StopEffect()
    {
        if (effectObject == null)
        {
            return;
        }

        for (int i = 0; i < particleSystems.Length; i++)
        {
            particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        effectObject.SetActive(false);
    }
}
