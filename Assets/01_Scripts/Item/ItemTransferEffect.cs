using DG.Tweening;
using UnityEngine;

public class ItemTransferEffect : MonoBehaviour
{
    [Header("연출")]
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private float height = 0.6f;
    [SerializeField] private Ease moveEase;

    private SpriteRenderer sr;
    private Vector3 baseScale;

    private PoolManager ownerPool;
    private Tween moveTween;

    private bool isReturning;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    // 외부에서 호출해 시각 아이템을 생성하고 효과를 재생하는 메서드
    public static void Play(ItemTransferEffect prefab, Sprite icon, Vector3 startPosition, Transform target)
    {
        PoolManager pool = PoolManager.Instance;

        if (pool == null || prefab == null || icon == null || target == null)
        {
            return;
        }

        ItemTransferEffect effect = pool.GetPool(prefab);

        if (effect != null)
        {
            effect.PlayInternal(pool, icon, startPosition, target);
        }
    }

    private void PlayInternal(PoolManager pool, Sprite icon, Vector3 startPosition, Transform target)
    {
        KillTween();

        ownerPool = pool;
        isReturning = false;

        if (sr != null)
        {
            sr.sprite = icon;
            sr.enabled = true;
        }

        transform.position = startPosition;
        transform.localScale = baseScale;

        float progress = 0f;

        // 시작값, 값 변경, 목표값, 시간
        moveTween = DOTween.To(
            () => progress,             // 시작값
            value => progress = value,  // 값 변경
            1f,                         // 목표값
            Mathf.Max(0.01f, duration)) // 시간
            .SetEase(moveEase)
            .OnUpdate(() =>
            {
                if (target == null)
                {
                    ReturnToPool();
                    return;
                }

                float t = Mathf.Clamp01(progress);

                // 타겟의 현재 위치를 사용
                Vector3 position = Vector3.Lerp(startPosition, target.position, t);

                position.y += Mathf.Sin(t * Mathf.PI) * height;

                transform.position = position;

                float reduction = Mathf.InverseLerp(0.8f, 1f, t);

                transform.localScale = baseScale * Mathf.Lerp(1f, 0.5f, reduction);
            })
            .OnComplete(ReturnToPool);
    }

    private void ReturnToPool()
    {
        if (isReturning) return;

        isReturning = true;

        if (ownerPool != null)
        {
            ownerPool.ReturnPool(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void KillTween()
    {
        Tween tween = moveTween;
        moveTween = null;

        tween.Kill(false);
    }

    private void OnDisable()
    {
        KillTween();

        transform.localScale = baseScale;

        if (sr != null)
        {
            sr.sprite = null;
            sr.enabled = false;
        }
    }
}
