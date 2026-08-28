using UnityEngine;
using UnityEngine.AI;

// NavMesh 이동 및 주문 결제 의존성을 상태 객체에 제공
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CustomerStateMachine))]
[RequireComponent(typeof(CustomerQueueMovement))]
public sealed class CustomerController : MonoBehaviour
{
    private const int NormalAvoidancePriority = 50;
    private const int CheckoutApproachAvoidancePriority = 5;
    private const int CheckoutAvoidancePriority = 10;
    private const int ExitAvoidancePriority = 0;

    [SerializeField] private CustomerDataSO customerData;
    [SerializeField] private GameObject customerHudPrefab;
    [Header("Legacy Defaults")]
    [SerializeField] private CustomerOrder defaultOrder;
    [SerializeField, Min(0f)] private float paymentDuration = 1.5f;
    [SerializeField, Min(0.1f)] private float patienceDuration = 600f;
    [SerializeField, Min(0.1f)] private float exitTimeout = 30f;

    private NavMeshAgent agent;
    private float defaultAgentSpeed;
    private float defaultAgentRadius;
    private ObstacleAvoidanceType defaultObstacleAvoidance;
    private Collider[] colliders;
    private bool[] defaultColliderStates;
    private Renderer[] renderers;
    private MaterialPropertyBlock[] defaultPropertyBlocks;
    private CustomerStateMachine stateMachine;
    private Transform exitTurnPoint;
    private Transform exitPoint;
    private ICustomerInventory inventory;
    private ICustomerCurrency currency;
    private Vector3 navigationDestination;
    private bool hasNavigationDestination;
    private CustomerQueueMovement queueMovement;
    private float patienceElapsed;
    private float patienceBonusSeconds;
    private float patienceBonusPercent;
    private bool didPatienceExpire;
    private readonly CustomerRuntimeData runtimeData = new CustomerRuntimeData();

    public CustomerRuntimeData RuntimeData => runtimeData;
    public CustomerOrder Order => runtimeData.Order;
    public CustomerOrder DefaultOrder => customerData != null && customerData.DefaultOrder.IsValid
        ? customerData.DefaultOrder
        : defaultOrder;
    public ShopCustomerQueue Queue => runtimeData.SelectedQueue;
    public ShopCheckout Checkout => runtimeData.SelectedCheckout;
    public bool HasExitTurnPoint => exitTurnPoint != null;
    public CustomerStateMachine StateMachine => stateMachine;
    public bool IsPaymentCompleted => runtimeData.PaymentCompleted;
    public float PatienceElapsed => patienceElapsed;
    public float PatienceNormalized => Mathf.Clamp01(patienceElapsed / PatienceDuration);
    public bool DidPatienceExpire => didPatienceExpire;
    public bool HasInventoryService => inventory != null;
    public ICustomerInventory InventoryService => inventory;
    public float PaymentDuration
    {
        get
        {
            float baseDuration = customerData != null ? customerData.PaymentDuration : paymentDuration;
            return Mathf.Max(0f, baseDuration * (Checkout != null ? Checkout.PaymentDurationMultiplier : 1f));
        }
    }
    public float PatienceDuration
    {
        get
        {
            float baseDuration = customerData != null ? customerData.PatienceDuration : patienceDuration;
            return Mathf.Max(0.1f, baseDuration * (1f + patienceBonusPercent / 100f) + patienceBonusSeconds);
        }
    }
    public float ExitTimeout => customerData != null ? customerData.ExitTimeout : exitTimeout;
    public bool HasCheckoutOperator => Checkout != null && Checkout.HasOperator;
    public CustomerQueueMovement QueueMovement => queueMovement;

    public event System.Action<CustomerController> ExitCompleted;
    public event System.Action<CustomerController, string> ExitFailed;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        defaultAgentSpeed = agent.speed;
        defaultAgentRadius = agent.radius;
        defaultObstacleAvoidance = agent.obstacleAvoidanceType;
        colliders = GetComponentsInChildren<Collider>(true);
        defaultColliderStates = new bool[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            defaultColliderStates[i] = colliders[i].enabled;
        }
        renderers = GetComponentsInChildren<Renderer>(true);
        defaultPropertyBlocks = new MaterialPropertyBlock[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            defaultPropertyBlocks[i] = new MaterialPropertyBlock();
            renderers[i].GetPropertyBlock(defaultPropertyBlocks[i]);
        }
        stateMachine = GetComponent<CustomerStateMachine>();
        queueMovement = GetComponent<CustomerQueueMovement>();
        if (queueMovement == null)
        {
            // 기존 프리팹에도 새 대기열 컴포넌트를 안전하게 적용한다.
            queueMovement = gameObject.AddComponent<CustomerQueueMovement>();
        }

        CustomerPatienceView patienceView = GetComponent<CustomerPatienceView>();
        if (patienceView == null)
        {
            patienceView = gameObject.AddComponent<CustomerPatienceView>();
        }
        patienceView.Configure(customerHudPrefab);

        stateMachine.Initialize(this);
    }

    private void OnDisable()
    {
        Queue?.Leave(this);
        queueMovement?.Clear();
        hasNavigationDestination = false;

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
        exitTurnPoint = exitTurn;
        exitPoint = exit;
        CustomerOrder selectedOrder = order.IsValid ? order : DefaultOrder;
        runtimeData.Initialize(shopQueue, checkoutService, selectedOrder);
        inventory = inventoryService;
        currency = currencyService;
        agent.speed = customerData != null ? customerData.MovementSpeed : defaultAgentSpeed;

        if (Queue == null || Checkout == null || exitPoint == null || !Order.IsValid || !Queue.TryJoin(this))
        {
            stateMachine.Cancel();
            return false;
        }

        stateMachine.BeginVisit();
        return true;
    }

    public void ResetCustomer()
    {
        Queue?.Leave(this);
        exitTurnPoint = null;
        inventory = null;
        currency = null;
        queueMovement?.Clear();
        hasNavigationDestination = false;
        patienceElapsed = 0f;
        patienceBonusSeconds = 0f;
        patienceBonusPercent = 0f;
        didPatienceExpire = false;
        runtimeData.Reset();

        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = false;
            agent.avoidancePriority = NormalAvoidancePriority;
            agent.speed = defaultAgentSpeed;
            agent.radius = defaultAgentRadius;
            agent.obstacleAvoidanceType = defaultObstacleAvoidance;
        }

        RestoreColliders();
        RestoreRendererProperties();
    }

    // 데이터 에셋이 없는 런타임 테스트에서만 기본 주문을 주입한다.
    public void ConfigureDefaultOrder(CustomerOrder order)
    {
        defaultOrder = order;
    }

    public void ConfigurePatienceDuration(float duration)
    {
        patienceDuration = Mathf.Max(0.1f, duration);
    }

    public void SetPatienceBonusSeconds(float bonusSeconds)
    {
        patienceBonusSeconds = Mathf.Max(0f, bonusSeconds);
    }

    public void SetPatienceBonusPercent(float bonusPercent)
    {
        patienceBonusPercent = Mathf.Max(0f, bonusPercent);
    }

    public void SetNavigationRadius(float radius)
    {
        if (agent != null)
        {
            agent.radius = Mathf.Max(0.05f, radius);
            agent.obstacleAvoidanceType = defaultObstacleAvoidance;
        }
    }

    public bool MoveToQueueDestination(Vector3 destination) => TryMoveTo(destination);

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

    public void StopInQueue()
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped = true;
        agent.avoidancePriority = NormalAvoidancePriority;
    }

    public bool CanFulfillOrder()
    {
        return !runtimeData.PaymentCompleted
            && inventory != null
            && Order.IsValid
            && inventory.CanConsumeAll(Order.Items);
    }

    public bool TryCompletePayment()
    {
        if (runtimeData.PaymentCompleted || Checkout == null || !Checkout.HasOperator || inventory == null || currency == null || !Order.IsValid || !inventory.TryConsumeAll(Order.Items))
        {
            return false;
        }

        // 주문 재료를 모두 차감한 뒤에만 돈과 경험치를 함께 지급한다.
        currency.GrantReward(Order.Reward, Order.ExperienceReward);
        runtimeData.CompletePayment();
        return true;
    }

    // 계산대 이동·철거로 대기열이 닫힐 때 보상 없이 손님을 퇴장시킨다.
    public void ForceExitWithoutPayment()
    {
        if (RuntimeData.CurrentStateName == "Exit")
        {
            return;
        }

        stateMachine.ChangeState(new CustomerExitState(this));
    }

    public bool TryHandlePatienceTimeout()
    {
        if (runtimeData.PaymentCompleted)
        {
            return false;
        }

        patienceElapsed += Time.deltaTime;
        if (patienceElapsed < PatienceDuration)
        {
            return false;
        }

        didPatienceExpire = true;
        ApplyPatienceExpiredVisual();
        ForceExitWithoutPayment();
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
        if (Checkout != null)
        {
            Checkout.OperatorPresenceChanged += handler;
        }
    }

    public void UnsubscribeOperatorPresenceChanged(System.Action handler)
    {
        if (Checkout != null)
        {
            Checkout.OperatorPresenceChanged -= handler;
        }
    }

    public void CompleteExit()
    {
        ExitCompleted?.Invoke(this);
    }

    public void FailExit(string reason)
    {
        Debug.LogWarning($"Customer exit failed: {reason}. The customer will be despawned.", this);
        ExitFailed?.Invoke(this, reason);
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

    private void ApplyPatienceExpiredVisual()
    {
        if (renderers == null)
        {
            return;
        }

        Color tint = new Color(1f, 0.2f, 0.2f, 1f);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || renderer.sharedMaterial == null)
            {
                continue;
            }

            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock);
            if (renderer.sharedMaterial.HasProperty("_BaseColor"))
            {
                propertyBlock.SetColor("_BaseColor", tint);
            }
            else if (renderer.sharedMaterial.HasProperty("_Color"))
            {
                propertyBlock.SetColor("_Color", tint);
            }
            else
            {
                continue;
            }

            renderer.SetPropertyBlock(propertyBlock);
        }
    }

    private void RestoreRendererProperties()
    {
        if (renderers == null || defaultPropertyBlocks == null)
        {
            return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].SetPropertyBlock(defaultPropertyBlocks[i]);
            }
        }
    }

    private bool TryMoveTo(Vector3 destination, int avoidancePriority = NormalAvoidancePriority)
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            return false;
        }

        // 같은 목적지를 다시 요청하는 대기열 상태 전환에서는 경로를 재계산하지 않는다.
        if (hasNavigationDestination && (navigationDestination - destination).sqrMagnitude <= 0.0001f)
        {
            agent.isStopped = false;
            agent.avoidancePriority = avoidancePriority;
            return true;
        }

        NavMeshPath path = new NavMeshPath();
        if (!agent.CalculatePath(destination, path) || path.status != NavMeshPathStatus.PathComplete)
        {
            return false;
        }

        agent.isStopped = false;
        agent.avoidancePriority = avoidancePriority;
        bool destinationSet = agent.SetDestination(destination);
        if (destinationSet)
        {
            navigationDestination = destination;
            hasNavigationDestination = true;
        }

        return destinationSet;
    }
}
