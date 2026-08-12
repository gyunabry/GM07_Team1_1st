using UnityEngine;
using UnityEngine.AI;

// NavMesh 이동 및 주문 결제 의존성을 상태 객체에 제공
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CustomerStateMachine))]
public sealed class CustomerController : MonoBehaviour
{
    private const int NormalAvoidancePriority = 50;
    private const int CheckoutApproachAvoidancePriority = 5;
    private const int CheckoutAvoidancePriority = 10;
    private const int ExitAvoidancePriority = 0;

    [SerializeField] private CustomerOrder defaultOrder;
    [SerializeField, Min(0f)] private float paymentDuration = 1.5f;

    private NavMeshAgent agent;
    private float defaultAgentRadius;
    private ObstacleAvoidanceType defaultObstacleAvoidance;
    private Collider[] colliders;
    private bool[] defaultColliderStates;
    private CustomerStateMachine stateMachine;
    private ShopCustomerQueue queue;
    private ShopCheckout checkout;
    private Transform exitTurnPoint;
    private Transform exitPoint;
    private ICustomerInventory inventory;
    private ICustomerCurrency currency;
    private Vector3 queueDestination;
    private bool hasQueueDestination;
    private bool paymentCompleted;

    public CustomerOrder Order { get; private set; }
    public ShopCustomerQueue Queue => queue;
    public ShopCheckout Checkout => checkout;
    public bool HasExitTurnPoint => exitTurnPoint != null;
    public CustomerStateMachine StateMachine => stateMachine;
    public bool IsPaymentCompleted => paymentCompleted;
    public bool HasInventoryService => inventory != null;
    public float PaymentDuration => paymentDuration;
    public bool HasCheckoutOperator => checkout != null && checkout.HasOperator;

    public event System.Action<CustomerController> ExitedShop;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        defaultAgentRadius = agent.radius;
        defaultObstacleAvoidance = agent.obstacleAvoidanceType;
        colliders = GetComponentsInChildren<Collider>(true);
        defaultColliderStates = new bool[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            defaultColliderStates[i] = colliders[i].enabled;
        }
        stateMachine = GetComponent<CustomerStateMachine>();
        stateMachine.Initialize(this);
    }

    private void OnDisable()
    {
        queue?.Leave(this);
        queue = null;
        hasQueueDestination = false;

        // 풀에 보관하는 동안 마지막 NavMesh 위치를 유지한 채 다시 생성되지 않게 한다.
        if (agent != null)
        {
            agent.enabled = false;
        }
    }

    // PoolManager에서 대여한 직후 호출
    public bool OnSpawned(ShopCustomerQueue shopQueue, ShopCheckout checkoutService, Transform exitTurn, Transform exit, CustomerOrder order, ICustomerInventory inventoryService, ICustomerCurrency currencyService)
    {
        if (agent != null && !agent.enabled)
        {
            // 입구 가장자리에서는 군중용으로 넓어진 반경(0.65)으로 Agent를 만들 수 없다.
            // Agent 활성화 전에 프리팹의 기본 반경과 회피 설정을 먼저 복원한다.
            agent.radius = defaultAgentRadius;
            agent.obstacleAvoidanceType = defaultObstacleAvoidance;
            agent.enabled = true;
        }

        ResetCustomer();
        queue = shopQueue;
        checkout = checkoutService;
        exitTurnPoint = exitTurn;
        exitPoint = exit;
        Order = order.IsValid ? order : defaultOrder;
        inventory = inventoryService;
        currency = currencyService;

        if (queue == null || checkout == null || exitPoint == null || !Order.IsValid || !queue.TryJoin(this))
        {
            stateMachine.Cancel();
            return false;
        }

        stateMachine.BeginVisit();
        return true;
    }

    public void ResetCustomer()
    {
        queue?.Leave(this);
        queue = null;
        checkout = null;
        exitTurnPoint = null;
        inventory = null;
        currency = null;
        paymentCompleted = false;
        hasQueueDestination = false;
        Order = default;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = false;
            agent.avoidancePriority = NormalAvoidancePriority;
            agent.radius = defaultAgentRadius;
            agent.obstacleAvoidanceType = defaultObstacleAvoidance;
        }

        RestoreColliders();
    }

    public void SetQueueDestination(Vector3 destination)
    {
        queueDestination = destination;
        hasQueueDestination = true;
        TryMoveTo(destination);
    }

    public void SetNavigationRadius(float radius)
    {
        if (agent != null)
        {
            agent.radius = Mathf.Max(0.05f, radius);
            agent.obstacleAvoidanceType = defaultObstacleAvoidance;
        }
    }

    public bool MoveToQueueDestination()
    {
        return hasQueueDestination && TryMoveTo(queueDestination);
    }

    public bool MoveToExit()
    {
        PrepareForExit();
        return exitPoint != null && TryMoveTo(exitPoint.position, ExitAvoidancePriority);
    }

    public bool MoveToCheckout(Vector3 destination, float radius)
    {
        SetNavigationRadius(radius);
        return TryMoveTo(destination, CheckoutApproachAvoidancePriority);
    }

    public bool MoveToExitTurnPoint()
    {
        PrepareForExit();
        return exitTurnPoint != null && TryMoveTo(exitTurnPoint.position, ExitAvoidancePriority);
    }

    public bool HasArrived()
    {
        return agent != null && agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    public void StopAtCheckout()
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            return;
        }

        // 계산 중인 손님은 최우선 회피 대상으로 두어 뒤 손님에게 밀려나지 않게 한다.
        agent.isStopped = true;
        agent.avoidancePriority = CheckoutAvoidancePriority;
    }

    public bool TryCompletePayment()
    {
        if (paymentCompleted || checkout == null || !checkout.HasOperator || inventory == null || currency == null || !Order.IsValid || !inventory.TryConsumeAll(Order.Items))
        {
            return false;
        }

        // 주문 재료를 모두 차감한 뒤에만 돈과 경험치를 함께 지급한다.
        currency.GrantReward(Order.Reward, Order.ExperienceReward);
        paymentCompleted = true;
        return true;
    }

    public void SubscribeInventoryChanged(System.Action handler)
    {
        if (inventory != null)
        {
            inventory.InventoryChanged += handler;
        }
    }

    public void UnsubscribeInventoryChanged(System.Action handler)
    {
        if (inventory != null)
        {
            inventory.InventoryChanged -= handler;
        }
    }

    public void SubscribeOperatorPresenceChanged(System.Action handler)
    {
        if (checkout != null)
        {
            checkout.OperatorPresenceChanged += handler;
        }
    }

    public void UnsubscribeOperatorPresenceChanged(System.Action handler)
    {
        if (checkout != null)
        {
            checkout.OperatorPresenceChanged -= handler;
        }
    }

    public void ReturnToPool()
    {
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnPool(this);
        }
    }

    public void ReturnToPoolAfterExit()
    {
        ReturnToPool();
        ExitedShop?.Invoke(this);
    }

    private void PrepareForExit()
    {
        if (agent == null)
        {
            return;
        }

        // 퇴장 중에는 군중 회피와 물리 Collider를 끄고, Agent 반경도 최소화한다.
        agent.radius = 0.05f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        SetCollidersEnabled(false);
    }

    private void SetCollidersEnabled(bool enabled)
    {
        if (colliders == null) return;

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null && !colliders[i].isTrigger)
            {
                colliders[i].enabled = enabled;
            }
        }
    }

    private void RestoreColliders()
    {
        if (colliders == null || defaultColliderStates == null) return;

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = defaultColliderStates[i];
            }
        }
    }

    private bool TryMoveTo(Vector3 destination, int avoidancePriority = NormalAvoidancePriority)
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            return false;
        }

        NavMeshPath path = new NavMeshPath();
        if (!agent.CalculatePath(destination, path) || path.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }

        agent.isStopped = false;
        agent.avoidancePriority = avoidancePriority;
        return agent.SetDestination(destination);
    }
}
