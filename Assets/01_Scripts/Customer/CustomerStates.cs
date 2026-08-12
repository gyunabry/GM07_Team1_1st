// 계산대 대기열의 현재 슬롯으로 이동
public sealed class CustomerVisitState : ICustomerState
{
    private readonly CustomerController controller;
    public string Name => "Visit";
    public CustomerVisitState(CustomerController controller) => this.controller = controller;
    public void Enter()
    {
        bool moveStarted = controller.Queue != null && controller.Queue.IsFront(controller)
            ? controller.Queue.MoveFrontCustomerToCheckout()
            : controller.MoveToQueueDestination();

        if (!moveStarted) controller.StateMachine.Cancel();
    }
    public void Update()
    {
        if (controller.Queue != null && controller.Queue.IsInCheckoutRange(controller))
        {
            controller.StateMachine.ChangeState(new CustomerOrderState(controller));
        }
    }
    public void Exit() { }
}

// 줄의 맨 앞에 도착할 때까지 기다린 뒤 주문을 확정
public sealed class CustomerOrderState : ICustomerState
{
    private readonly CustomerController controller;
    public string Name => "Order";
    public CustomerOrderState(CustomerController controller) => this.controller = controller;
    public void Enter() { }
    public void Update()
    {
        if (controller.Queue != null && controller.Queue.IsFront(controller) && controller.Queue.IsInCheckoutRange(controller))
        {
            controller.StateMachine.ChangeState(new CustomerIdleState(controller));
        }
    }
    public void Exit() { }
}

// 재료 변경 이벤트를 구독하고, 자동 결제가 가능해지면 한 번만 결제
public sealed class CustomerIdleState : ICustomerState
{
    private readonly CustomerController controller;
    private bool inventoryChanged;
    private bool operatorPresenceChanged;
    private float paymentElapsed;
    private bool paymentReady;
    public string Name => "Idle";
    public CustomerIdleState(CustomerController controller) => this.controller = controller;
    public void Enter()
    {
        controller.StopAtCheckout();
        controller.SubscribeInventoryChanged(OnInventoryChanged);
        controller.SubscribeOperatorPresenceChanged(OnOperatorPresenceChanged);
        paymentElapsed = 0f;
        paymentReady = false;
    }
    public void Update()
    {
        // 계산 담당자가 도착한 순간부터만 계산 시간을 잰다.
        // 담당자가 자리를 비우면 진행 중인 계산도 취소한다.
        if (!controller.HasCheckoutOperator)
        {
            paymentElapsed = 0f;
            paymentReady = false;
            return;
        }

        if (!paymentReady)
        {
            paymentElapsed += UnityEngine.Time.deltaTime;
            if (paymentElapsed < controller.PaymentDuration) return;

            paymentReady = true;
            TryAutoPayment();
            return;
        }

        if (inventoryChanged || operatorPresenceChanged)
        {
            inventoryChanged = false;
            operatorPresenceChanged = false;
            TryAutoPayment();
        }
    }
    public void Exit() { controller.UnsubscribeInventoryChanged(OnInventoryChanged); controller.UnsubscribeOperatorPresenceChanged(OnOperatorPresenceChanged); }
    private void OnInventoryChanged() { inventoryChanged = true; }
    private void OnOperatorPresenceChanged() { operatorPresenceChanged = true; }
    private void TryAutoPayment()
    {
        if (controller.TryCompletePayment()) controller.StateMachine.ChangeState(new CustomerExitState(controller));
    }
}

// 대기열에서 제거된 손님을 출구까지 이동시키고 PoolManager에 반환
public sealed class CustomerExitState : ICustomerState
{
    private readonly CustomerController controller;
    private bool movingToFinalExit;
    public string Name => "Exit";
    public CustomerExitState(CustomerController controller) => this.controller = controller;
    public void Enter()
    {
        controller.Queue?.Leave(controller);
        movingToFinalExit = !controller.HasExitTurnPoint;
        bool moveStarted = movingToFinalExit ? controller.MoveToExit() : controller.MoveToExitTurnPoint();
        if (!moveStarted) controller.ReturnToPool();
    }
    public void Update()
    {
        if (!controller.HasArrived()) return;
        if (!movingToFinalExit)
        {
            movingToFinalExit = true;
            if (controller.MoveToExit()) return;
        }

        controller.ReturnToPoolAfterExit();
    }
    public void Exit() { }
}
