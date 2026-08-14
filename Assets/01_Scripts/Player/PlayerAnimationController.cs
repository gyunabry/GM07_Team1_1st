using UnityEngine;

//플레이어 AnimationController, NavMeshClickMove와 연결.
public class PlayerAnimationController : MonoBehaviour
{
    public enum PlayerAnimState
    {
        Idle, Walk
    }
    private static readonly int StateHash = Animator.StringToHash("State");

    [SerializeField] private Animator animator;

    private PlayerAnimState currentState = PlayerAnimState.Idle;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    public void SetState(PlayerAnimState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        animator.SetInteger(StateHash, (int)newState);
    }
}
