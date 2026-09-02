using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntegratedTransmitterTransferZone : MonoBehaviour
{
    [SerializeField] private IntegratedTransmitter transmitter;
    [SerializeField] private float transferInterval = 0.05f;
    [SerializeField] private LayerMask receiverLayer;

    private IInventoryProvider currentProvider;
    private Component currentProviderComponent;
    private Coroutine transferCoroutine;
    private WaitForSeconds transferWait;

    private readonly HashSet<Collider> currentColliders = new();

    private void Awake()
    {
        if (transmitter == null)
        {
            transmitter = GetComponentInParent<IntegratedTransmitter>();
        }

        transferWait = new WaitForSeconds(transferInterval);
    }

    private void OnDisable()
    {
        StopTransfer();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsReceiverLayer(other.gameObject.layer)) return;

        IInventoryProvider provider = other.GetComponentInParent<IInventoryProvider>();

        Component providerComponent = provider as Component;

        if (provider == null || providerComponent == null || provider.Inventory == null)
        {
            return;
        }

        if (currentProviderComponent != null && currentProviderComponent != providerComponent) return;

        currentProvider = provider;
        currentProviderComponent = providerComponent;
        currentColliders.Add(other);

        if (transferCoroutine == null)
        {
            if (currentProvider is PlayerInventory)
            {
                return;
            }

            transferCoroutine = StartCoroutine(TransferCo());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IInventoryProvider provider = other.GetComponentInParent<IInventoryProvider>();

        Component providerComponent = provider as Component;

        if (currentProviderComponent != null && currentProviderComponent != providerComponent) return;

        currentColliders.Remove(other);

        if (currentColliders.Count == 0)
        {
            StopTransfer();
        }
    }

    private IEnumerator TransferCo()
    {
        while (currentProviderComponent != null && currentProvider != null)
        {
            transmitter.TryGiveOne(currentProvider.Inventory);

            yield return transferWait;
        }

        transferCoroutine = null;
        currentProvider = null;
        currentProviderComponent = null;
        currentColliders.Clear();
    }

    private bool IsReceiverLayer(int layer)
    {
        return (receiverLayer.value & (1 << layer)) != 0;
    }

    private void StopTransfer()
    {
        if (transferCoroutine != null)
        {
            StopCoroutine(transferCoroutine);
            transferCoroutine = null;
        }

        currentProvider = null;
        currentProviderComponent = null;
    }
}
