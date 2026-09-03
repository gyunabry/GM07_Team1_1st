using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class Dropitem : MonoBehaviour, ICollectable
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private float lifetimeSec = 45f;

    [Header("자석 효과")]
    [SerializeField] private float magnetSpeed = 7.5f;
    [SerializeField] private float collectDistance = 0.25f;

    private Coroutine lifetimeCoroutine;
    private WaitForSeconds lifetimeWait;

    private Transform magnetTarget;
    private ItemInventory targetInventory;

    private Vector3 visualLocalPosition;

    public ItemDataSO Item { get; private set; }
    public int Amount { get; private set; } = 1;

    private void Awake()
    {
        lifetimeWait = new WaitForSeconds(lifetimeSec);

        if (sr != null)
        {
            visualLocalPosition = sr.transform.position;
        }
    }

    private void Update()
    {
        if (magnetTarget == null || targetInventory == null)
        {
            return;
        }

        // 대상 인벤토리가 가득 찬 상태라면 그대로 대기
        if (targetInventory.RemainingCapacity <= 0)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            magnetTarget.position,
            magnetSpeed * Time.deltaTime);

        // 성능을 위해 Distance 대신 거리의 제곱을 비교
        float collectDistanceSqr = collectDistance * collectDistance;

        if ((transform.position - magnetTarget.position).sqrMagnitude <= collectDistanceSqr)
        {
            TryCollect(targetInventory);
        }
    }

    public void Initialize(ItemDataSO item, int amount = 1)
    {
        Item = item;
        Amount = amount;

        KillTween();
        ItemFloating();

        if (sr != null)
        {
            sr.sprite = item != null ? item.Icon : null;
            sr.enabled = item != null && item.Icon != null;
        }

        RestartLifetime();
    }

    private void RestartLifetime()
    {
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
        }

        lifetimeCoroutine = StartCoroutine(LifetimeCo());
    }

    private IEnumerator LifetimeCo()
    {
        yield return lifetimeWait;

        lifetimeCoroutine = null;
        
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnPool(this);
        }
    }

    public void StartMagnet(Transform target, ItemInventory inventory)
    {
        if (target == null || inventory == null)
        {
            return;
        }

        magnetTarget = target;
        targetInventory = inventory;
    }

    public void StopMagnet(Transform target)
    {
        if (target == null) return;

        magnetTarget = null;
        targetInventory = null;
    }

    public bool TryCollect(ItemInventory target)
    {
        if (target == null || Item == null || Amount <= 0)
        {
            return false;
        }

        int added = target.Add(Item, Amount);

        if (added <= 0) return false;

        Amount -= added;

        if (Amount <= 0)
        {
            PoolManager.Instance.ReturnPool(this);
        }

        return true;
    }

    public int TryCollectAmount(int requestedAmount)
    {
        if (Item == null || Amount <= 0 || requestedAmount <= 0)
        {
            return 0;
        }

        int collected = Mathf.Min(Amount, requestedAmount);
        Amount -= collected;

        if (Amount <= 0)
        {
            PoolManager.Instance.ReturnPool(this);
        }

        return collected;
    }

    private void OnDisable()
    {
        KillTween();
        magnetTarget = null;
        targetInventory = null;

        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }

        Item = null;
        Amount = 0;

        if (sr != null)
        {
            sr.sprite = null;
            sr.enabled = false;
        }
    }

    // DOTween 추가
    private void ItemFloating()
    {
        if (sr == null) return;

        sr.transform.localPosition = visualLocalPosition;

        sr.transform.DOMoveY(visualLocalPosition.y + 0.3f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    private void KillTween()
    {
        sr.transform.DOKill();
    }
}
