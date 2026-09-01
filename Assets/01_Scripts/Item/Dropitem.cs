using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class Dropitem : MonoBehaviour, ICollectable
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private float lifetimeSec = 45f;

    private Coroutine lifetimeCoroutine;
    private WaitForSeconds lifetimeWait;

    public ItemDataSO Item { get; private set; }
    public int Amount { get; private set; } = 1;

    private void Awake()
    {
        lifetimeWait = new WaitForSeconds(lifetimeSec);
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

    // DOTween Ãß°¡
    private void ItemFloating()
    {
        sr.transform.DOMoveY(0.3f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
    }

    private void KillTween()
    {
        sr.transform.DOKill();
    }
}
