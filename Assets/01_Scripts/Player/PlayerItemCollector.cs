using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    [SerializeField] private LayerMask collectableLayer;

    private PlayerInventory playerInventory;

    private void Awake()
    {
        playerInventory = GetComponentInParent<PlayerInventory>();
    }

    private void OnTriggerEnter(Collider other)
    {
        TryCollect(other);
    }

    // 플레이어가 이미 상호작용 범위 안에 있을 때 아이템이 생기는 케이스 대비
    private void OnTriggerStay(Collider other)
    {
        TryCollect(other);
    }

    private void TryCollect(Collider other)
    {
        if (other == null || playerInventory == null)
        {
            return;
        }

        if (!IsCollectableLayer(other.gameObject.layer))
        {
            return;
        }

        ICollectable collectable = other.GetComponentInParent<ICollectable>();
        if (collectable == null) return;

        collectable.TryCollect(playerInventory.Inventory);
    }

    private bool IsCollectableLayer(int layer)
    {
        return (collectableLayer.value & (1 << layer)) != 0;
    }
}
