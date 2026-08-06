using UnityEngine;

// 손님의 현재 상태를 소유하고 상태 전환을 단일 경로로 제한
public sealed class CustomerStateMachine : MonoBehaviour
{
    private ICustomerState currentState;
    private CustomerController controller;

    public string CurrentStateName => currentState == null ? string.Empty : currentState.Name;

    public void Initialize(CustomerController owner)
    {
        controller = owner;
    }

    public void BeginVisit()
    {
        if (controller == null)
        {
            Debug.LogError("CustomerStateMachine requires a CustomerController.", this);
            return;
        }

        ChangeState(new CustomerVisitState(controller));
    }

    public void ChangeState(ICustomerState nextState)
    {
        if (nextState == null)
        {
            return;
        }

        currentState?.Exit();
        currentState = nextState;
        currentState.Enter();
    }

    public void Cancel()
    {
        ChangeState(new CustomerExitState(controller));
    }

    private void Update()
    {
        currentState?.Update();
    }

    private void OnDisable()
    {
        currentState?.Exit();
        currentState = null;
    }
}
