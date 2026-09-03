using UnityEngine;

/// <summary>
/// 판매대 직원의 설치, 대기, 판매, 판매 완료 애니메이션을 제어합니다.
/// </summary>
[DisallowMultipleComponent]
public sealed class SalesEmployeeAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private bool isSelling;
    private float completionEndTime;

    private void Awake()
    {
        animator ??= GetComponentInChildren<Animator>(true);
    }

    private void OnEnable()
    {
        if (animator != null)
        {
            animator.Play("Spawn", 0, 0f);
        }
    }

    private void Update()
    {
        if (animator == null || Time.time < completionEndTime)
        {
            return;
        }

        string stateName = isSelling ? "Wave" : "Idle";
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            animator.Play(stateName, 0, 0f);
        }
    }

    public void SetSelling(bool isSelling)
    {
        this.isSelling = isSelling;
    }

    public void PlaySaleCompleted()
    {
        if (animator == null)
        {
            return;
        }

        isSelling = false;
        animator.Play("Cast Spell 02", 0, 0f);
        completionEndTime = Time.time + animator.GetCurrentAnimatorStateInfo(0).length;
    }
}
