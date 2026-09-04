using UnityEngine;

/// <summary>
/// 판매대 직원의 설치, 대기, 판매, 판매 완료 애니메이션을 제어합니다.
/// </summary>
[DisallowMultipleComponent]
public sealed class SalesEmployeeAnimationController : MonoBehaviour
{
    private static readonly int SpawnHash = Animator.StringToHash("Base Layer.Spawn");
    private static readonly int IdleHash = Animator.StringToHash("Base Layer.Idle");
    private static readonly int WaveHash = Animator.StringToHash("Base Layer.Wave");
    private static readonly int CastSpell02Hash = Animator.StringToHash("Base Layer.Cast Spell 02");

    [SerializeField] private Animator animator;
    private bool isSelling;
    private float spawnEndTime;
    private float completionEndTime;

    private void Awake()
    {
        FindAnimator();
    }

    private void OnEnable()
    {
        if (FindAnimator())
        {
            animator.Play(SpawnHash, 0, 0f);
            animator.Update(0f);
            spawnEndTime = Time.time + animator.GetCurrentAnimatorStateInfo(0).length;
        }
    }

    private void Update()
    {
        if (!FindAnimator() || Time.time < spawnEndTime || Time.time < completionEndTime)
        {
            return;
        }

        PlayPersistentState();
    }

    public void SetSelling(bool isSelling)
    {
        this.isSelling = isSelling;

        if (FindAnimator() && Time.time >= spawnEndTime && Time.time >= completionEndTime)
        {
            PlayPersistentState();
        }
    }

    public void PlaySaleCompleted()
    {
        if (!FindAnimator())
        {
            return;
        }

        isSelling = false;
        animator.Play(CastSpell02Hash, 0, 0f);
        animator.Update(0f);
        completionEndTime = Time.time + animator.GetCurrentAnimatorStateInfo(0).length;
    }

    private bool FindAnimator()
    {
        animator ??= GetComponentInChildren<Animator>(true);
        return animator != null;
    }

    private void PlayPersistentState()
    {
        int stateHash = isSelling ? WaveHash : IdleHash;
        if (animator.GetCurrentAnimatorStateInfo(0).fullPathHash != stateHash)
        {
            animator.Play(stateHash, 0, 0f);
        }
    }
}
