using System.Collections;
using UnityEngine;

public class TransmitterTransferZone : MonoBehaviour
{
    [Header("전송 설정")]
    [SerializeField] private Transmitter transmitter;
    [SerializeField] private float inputInterval = 0.1f;
    [SerializeField] private LayerMask sourceLayer;

    [Header("전송 연출")]
    [SerializeField] private ItemTransferEffect transferEffectPrefab;
    [SerializeField] private Transform transferAnchor;

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
            int moved = transmitter.TryReceiveOne(currentProvider.Inventory, out ItemDataSO item);
            
            if (moved > 0 &&
                item != null &&
                    currentProviderComponent is PlayerInventory player)
            {
                AudioManager.Instance.PlaySFX(ESFXType.Inven_Supply);

                Transform source = player.TransferAnchor != null 
                    ? player.TransferAnchor
                    : player.transform;

                Transform destination = transferAnchor != null
                    ? transferAnchor
                    : transmitter.transform;

                ItemTransferEffect.Play(
                    transferEffectPrefab,
                    item.Icon,
                    source.position,
                    destination
                );
            }

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
