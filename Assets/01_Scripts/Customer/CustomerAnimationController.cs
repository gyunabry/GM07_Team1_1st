using UnityEngine;

public class CustomerAnimationController : MonoBehaviour
{
    public enum CustomerAnimState
    {
        Idle, Walk
    }
    private static readonly int IsWalkHash = Animator.StringToHash("IsWalk");

    [SerializeField] private Animator animator;

    private CustomerAnimState currentState = CustomerAnimState.Idle;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    public void SetState(CustomerAnimState newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        bool isWalking = (currentState == CustomerAnimState.Walk);
        animator.SetBool(IsWalkHash, isWalking);
    }
}
