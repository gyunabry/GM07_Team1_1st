using UnityEngine;

// 손님 한 명의 대기열 자리와 해당 자리로의 복귀를 관리한다.
[RequireComponent(typeof(CustomerController))]
public sealed class CustomerQueueMovement : MonoBehaviour
{
    private CustomerController controller;
    private Vector3 destination;
    private bool hasDestination;

    public bool HasDestination => hasDestination;

    private void Awake()
    {
        controller = GetComponent<CustomerController>();
    }

    public bool SetDestination(Vector3 value)
    {
        destination = value;
        hasDestination = true;
        return controller != null && controller.MoveToQueueDestination(value);
    }

    public bool ResumeMovement()
    {
        return hasDestination && controller != null && controller.MoveToQueueDestination(destination);
    }

    public bool IsOutsideDestination(float tolerance)
    {
        if (!hasDestination)
        {
            return false;
        }

        Vector3 offset = transform.position - destination;
        offset.y = 0f;
        return offset.sqrMagnitude > tolerance * tolerance;
    }

    public void Clear()
    {
        hasDestination = false;
    }
}
