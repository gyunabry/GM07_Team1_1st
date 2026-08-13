using UnityEngine;

public class CustomerAnimationController : MonoBehaviour
{
    public enum CustomerAnimState
    {
        Idle, Walk
    }
    private static readonly int StateHash = Animator.StringToHash("State");

    [SerializeField] private Animator animator;

    private CustomerAnimState currentState = CustomerAnimState.Idle;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetState(CustomerAnimState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        animator.SetInteger(StateHash, (int)newState);
    }
}
