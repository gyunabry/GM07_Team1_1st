using UnityEngine;

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
        animator = GetComponent<Animator>();
    }

    public void SetState(PlayerAnimState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        animator.SetInteger(StateHash, (int)newState);
    }
}
