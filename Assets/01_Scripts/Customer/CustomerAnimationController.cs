using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class CustomerAnimationController : MonoBehaviour
{
    public enum CustomerAnimState
    {
        Idle, Walk
    }
    private static readonly int IsWalkHash = Animator.StringToHash("IsWalk");

    [SerializeField] private Animator animator;

    private CustomerAnimState currentState = CustomerAnimState.Idle;
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        animator = GetComponentInChildren<Animator>(false);
    }

    private void Update()
    {
        if (agent == null)
        {
            return;
        }

        Animator activeAnimator = GetComponentInChildren<Animator>(false);
        if (activeAnimator != animator)
        {
            animator = activeAnimator;
        }

        bool isWalking = agent.enabled && agent.isOnNavMesh && !agent.isStopped && agent.velocity.sqrMagnitude > 0.01f;
        SetState(isWalking ? CustomerAnimState.Walk : CustomerAnimState.Idle);
    }

    public void SetState(CustomerAnimState newState)
    {
        currentState = newState;

        if (animator != null)
        {
            animator.SetBool(IsWalkHash, currentState == CustomerAnimState.Walk);
        }
    }
}
