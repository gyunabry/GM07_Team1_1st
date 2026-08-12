// 풀에서 대여된 손님 한 명의 현재 진행 상태를 보관한다.
public sealed class CustomerRuntimeData
{
    public string CurrentStateName { get; private set; } = string.Empty;
    public ShopCustomerQueue SelectedQueue { get; private set; }
    public ShopCheckout SelectedCheckout { get; private set; }
    public CustomerOrder Order { get; private set; }
    public bool PaymentCompleted { get; private set; }

    public void Initialize(ShopCustomerQueue queue, ShopCheckout checkout, CustomerOrder order)
    {
        SelectedQueue = queue;
        SelectedCheckout = checkout;
        Order = order;
        PaymentCompleted = false;
        CurrentStateName = string.Empty;
    }

    public void SetState(string stateName)
    {
        CurrentStateName = stateName ?? string.Empty;
    }

    public void CompletePayment()
    {
        PaymentCompleted = true;
    }

    public void Reset()
    {
        SelectedQueue = null;
        SelectedCheckout = null;
        Order = default;
        PaymentCompleted = false;
        CurrentStateName = string.Empty;
    }
}
