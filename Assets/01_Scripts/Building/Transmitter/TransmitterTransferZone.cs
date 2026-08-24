using System.Collections;
using UnityEngine;

public class TransmitterTransferZone : MonoBehaviour
{
    [SerializeField] private Transmitter transmitter;
    [SerializeField] private float inputInterval = 0.05f;
    [SerializeField] private LayerMask sourceLayer;

    private IInventoryProvider currentProvider;
    private Component currentProviderComponent;
    private Coroutine inputCoroutine;
    private WaitForSeconds inputWait;

    private void Awake()
    {
        if (transmitter == null)
        {
            transmitter = GetComponentInParent<Transmitter>();
        }

        inputWait = new WaitForSeconds(inputInterval);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsSourceLayer(other.gameObject.layer)) return;

        IInventoryProvider provider = other.GetComponentInParent<IInventoryProvider>();

        if (provider == null || provider.Inventory == null) return;

        currentProvider = provider;
        currentProviderComponent = provider as Component;

        if (inputCoroutine == null)
        {
            inputCoroutine = StartCoroutine(InputCo());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IInventoryProvider provider = other.GetComponentInParent<IInventoryProvider>();

        if (!ReferenceEquals(provider, currentProvider)) return;

        StopInput();
    }

    private IEnumerator InputCo()
    {
        while (currentProviderComponent != null && currentProvider != null)
        {
            transmitter.TryReceiveOne(currentProvider.Inventory);
            yield return inputWait;
        }
    }

    private bool IsSourceLayer(int layer)
    {
        return (sourceLayer.value & (1 << layer)) != 0;
    }

    private void StopInput()
    {
        if (inputCoroutine != null)
        {
            StopCoroutine(inputCoroutine);
            inputCoroutine = null;
        }

        currentProvider = null;
        currentProviderComponent = null;
    }
}
