using UnityEngine;
using UnityEngine.AI;

// NavMesh 이동 및 주문 결제 의존성을 상태 객체에 제공
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CustomerStateMachine))]
public sealed class CustomerController : MonoBehaviour
{
    [SerializeField] private CustomerOrder defaultOrder;

    private NavMeshAgent agent;
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

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        stateMachine = GetComponent<CustomerStateMachine>();
        stateMachine.Initialize(this);
    }

    private void OnDisable()
    {
        queue?.Leave(this);
        queue = null;
        hasQueueDestination = false;
    }

    // PoolManager에서 대여한 직후 호출
    public bool OnSpawned(ShopCustomerQueue shopQueue, ShopCheckout checkoutService, Transform exitTurn, Transform exit, CustomerOrder order, ICustomerInventory inventoryService, ICustomerCurrency currencyService)
    {
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
        }
    }

    public void SetQueueDestination(Vector3 destination)
    {
        queueDestination = destination;
        hasQueueDestination = true;
        TryMoveTo(destination);
    }

    public bool MoveToQueueDestination()
    {
        return hasQueueDestination && TryMoveTo(queueDestination);
    }

    public bool MoveToExit()
    {
        return exitPoint != null && TryMoveTo(exitPoint.position);
    }

    public bool MoveToExitTurnPoint()
    {
        return exitTurnPoint != null && TryMoveTo(exitTurnPoint.position);
    }

    public bool HasArrived()
    {
        return agent != null && agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    public bool TryCompletePayment()
    {
        if (paymentCompleted || checkout == null || !checkout.HasOperator || inventory == null || currency == null || !Order.IsValid || !inventory.TryConsumeAll(Order.Items))
        {
            return false;
        }

        currency.Grant(Order.Reward);
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

    private bool TryMoveTo(Vector3 destination)
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
        return agent.SetDestination(destination);
    }
}
