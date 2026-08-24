using UnityEngine;

public class EnemyAnimationController : MonoBehaviour
{
    private static int moveHash = Animator.StringToHash("Move");
    private static int hitHash = Animator.StringToHash("Hit");
    private static int dieHash = Animator.StringToHash("Die");

    [SerializeField] private Animator animator;
    [SerializeField] private Enemy enemy;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (enemy == null) enemy = GetComponent<Enemy>();
    }

    public void SetMoveSpeed(float speed)
    {
        animator.SetFloat(moveHash, speed);
    }

    public void PlayHit()
    {
        animator.SetTrigger(hitHash);
    }

    public void PlayDie()
    {
        animator.ResetTrigger(hitHash);
        animator.SetTrigger(dieHash);
    }

    public void AnimationEvent_CompleteDeath()
    {
        if (enemy.stateController?.nowState is EnemyDieState dieState)
        {
            dieState.CompleteDeath();
        }
    }
}
