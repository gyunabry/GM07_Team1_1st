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
    [Header("Movement Animation")]
    [SerializeField, Min(0f)] private float walkStartSpeed = 0.25f;
    [SerializeField, Min(0f)] private float walkStopSpeed = 0.12f;
    [SerializeField, Min(0f)] private float walkStartDelay = 0.35f;
    [SerializeField, Min(0f)] private float walkStopDelay = 0.2f;

    private CustomerAnimState currentState = CustomerAnimState.Idle;
    private NavMeshAgent agent;
    private float stateChangeElapsed;

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

        bool canWalk = agent.enabled && agent.isOnNavMesh && !agent.isStopped;
        float speed = agent.velocity.magnitude;
        bool shouldWalk = canWalk && speed >= walkStartSpeed;
        bool shouldIdle = !canWalk || speed < Mathf.Min(walkStopSpeed, walkStartSpeed);

        if (currentState == CustomerAnimState.Idle)
        {
            stateChangeElapsed = shouldWalk ? stateChangeElapsed + Time.deltaTime : 0f;
            if (stateChangeElapsed >= walkStartDelay)
            {
                SetState(CustomerAnimState.Walk);
            }

            return;
        }

        stateChangeElapsed = shouldIdle ? stateChangeElapsed + Time.deltaTime : 0f;
        if (stateChangeElapsed >= walkStopDelay)
        {
            SetState(CustomerAnimState.Idle);
        }
    }

    public void SetState(CustomerAnimState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            stateChangeElapsed = 0f;
        }

        if (animator != null)
        {
            animator.SetBool(IsWalkHash, currentState == CustomerAnimState.Walk);
        }
    }
}
