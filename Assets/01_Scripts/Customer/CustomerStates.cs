// 계산대 대기열의 현재 슬롯으로 이동
public sealed class CustomerVisitState : ICustomerState
{
    private readonly CustomerController controller;
    public string Name => "Visit";
    public CustomerVisitState(CustomerController controller) => this.controller = controller;
    public void Enter() { if (!controller.MoveToQueueDestination()) controller.StateMachine.Cancel(); }
    public void Update() { if (controller.HasArrived()) controller.StateMachine.ChangeState(new CustomerOrderState(controller)); }
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
        if (controller.Queue != null && controller.Queue.IsFront(controller) && controller.HasArrived())
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
    public string Name => "Idle";
    public CustomerIdleState(CustomerController controller) => this.controller = controller;
    public void Enter() { controller.SubscribeInventoryChanged(OnInventoryChanged); TryAutoPayment(); }
    public void Update() { if (inventoryChanged) { inventoryChanged = false; TryAutoPayment(); } }
    public void Exit() { controller.UnsubscribeInventoryChanged(OnInventoryChanged); }
    private void OnInventoryChanged() { inventoryChanged = true; }
    private void TryAutoPayment()
    {
        if (controller.TryCompletePayment()) controller.StateMachine.ChangeState(new CustomerExitState(controller));
    }
}

// 대기열에서 제거된 손님을 출구까지 이동시키고 PoolManager에 반환
public sealed class CustomerExitState : ICustomerState
{
    private readonly CustomerController controller;
    public string Name => "Exit";
    public CustomerExitState(CustomerController controller) => this.controller = controller;
    public void Enter() { controller.Queue?.Leave(controller); if (!controller.MoveToExit()) controller.ReturnToPool(); }
    public void Update() { if (controller.HasArrived()) controller.ReturnToPool(); }
    public void Exit() { }
}
