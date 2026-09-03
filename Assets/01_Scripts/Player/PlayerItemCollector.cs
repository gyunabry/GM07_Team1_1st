using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    [Header("수집 대상 레이어")]
    [SerializeField] private LayerMask collectableLayer;

    [Header("획득 반경")]
    [SerializeField] private float magnetRange = 2.4f;

    private PlayerInventory playerInventory;
    private SphereCollider collectionCollider;

    private float baseRange;
    private float bonusRangeRate;

    private bool rangeInitialzed;

    public float BaseRange
    {
        get
        {
            EnsureRangeInitialized();
            return baseRange;
        }
    }

    public float Range
    {
        get
        {
            EnsureRangeInitialized();
            return collectionCollider.radius;
        }
    }

    private void Awake()
    {
        playerInventory = GetComponentInParent<PlayerInventory>();

        EnsureRangeInitialized();
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

    private void OnTriggerExit(Collider other)
    {
        if (!IsCollectableLayer(other.gameObject.layer))
        {
            return;
        }

        Dropitem dropItem = other.GetComponentInParent<Dropitem>();
        if (dropItem == null) return;

        dropItem.StopMagnet(transform);
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

        Dropitem dropItem = other.GetComponentInParent<Dropitem>();
        if (dropItem == null) return;

        dropItem.StartMagnet(transform, playerInventory.Inventory);
    }

    private void EnsureRangeInitialized()
    {
        if (rangeInitialzed) return;

        collectionCollider = GetComponent<SphereCollider>();

        if (collectionCollider == null) return;

        baseRange = Mathf.Max(0f, collectionCollider.radius);
        bonusRangeRate = 0f;
        rangeInitialzed = true;
    }

    public void AddRangeBonusRate(float rate)
    {
        EnsureRangeInitialized();
        if (!rangeInitialzed) return;

        bonusRangeRate += Mathf.Max(0f, rate);
        ApplyRange();
    }

    public void ResetRangeBonus()
    {
        EnsureRangeInitialized();
        if (!rangeInitialzed) return;

        bonusRangeRate = 0f;
        ApplyRange();
    }

    private void ApplyRange()
    {
        collectionCollider.radius = baseRange * (1f + bonusRangeRate);
    }

    private bool IsCollectableLayer(int layer)
    {
        return (collectableLayer.value & (1 << layer)) != 0;
    }
}
