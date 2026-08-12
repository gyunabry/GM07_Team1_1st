using System.Collections.Generic;
using UnityEngine;

// 하나의 상점 계산대 앞 대기열과 각 손님의 NavMesh 목적지를 관리
public sealed class ShopCustomerQueue : MonoBehaviour
{
    [SerializeField] private Transform checkoutFront;
    [SerializeField] private int maxCustomers = 20;
    [SerializeField, Min(0.1f)] private float crowdAgentRadius = 0.65f;
    [SerializeField, Min(0.1f)] private float frontCustomerRadius = 0.35f;
    [SerializeField, Min(0.1f)] private float checkoutAcceptanceRadius = 0.9f;

    private readonly List<CustomerController> customers = new List<CustomerController>();

    public int Count => customers.Count;
    public int Capacity => maxCustomers;

    public void Configure(Transform front, float slotSpacing, int capacity)
    {
        checkoutFront = front;
        crowdAgentRadius = Mathf.Max(0.1f, slotSpacing * 0.52f);
        maxCustomers = Mathf.Max(1, capacity);
    }

    public bool TryJoin(CustomerController customer)
    {
        if (customer == null || checkoutFront == null || customers.Contains(customer) || customers.Count >= maxCustomers)
        {
            return false;
        }

        customers.Add(customer);
        // 모두 판매대 정면을 향한다. 고정 대기 위치를 배정하지 않으므로,
        // NavMeshAgent의 회피가 손님들을 판매대 앞에서 자연스럽게 뭉치게 한다.
        customer.SetNavigationRadius(crowdAgentRadius);
        customer.SetQueueDestination(checkoutFront.position);
        return true;
    }

    public void Leave(CustomerController customer)
    {
        if (customer != null && customers.Remove(customer))
        {
            // 앞 손님이 떠난 뒤 다음 손님에게 목적지를 다시 알려 군중 사이에서 이동을 재개하게 한다.
            // 씬 종료 시 기준 Transform이 먼저 파괴될 수 있으므로, 그때는 목적지를 갱신하지 않는다.
            MoveFrontCustomerToCheckout();
        }
    }

    public bool IsFront(CustomerController customer)
    {
        return customers.Count > 0 && customers[0] == customer;
    }

    public bool IsInCheckoutRange(CustomerController customer)
    {
        if (customer == null || checkoutFront == null)
        {
            return false;
        }

        Vector3 offset = customer.transform.position - checkoutFront.position;
        offset.y = 0f;
        return offset.sqrMagnitude <= checkoutAcceptanceRadius * checkoutAcceptanceRadius;
    }

    public bool MoveFrontCustomerToCheckout()
    {
        if (customers.Count == 0 || checkoutFront == null)
        {
            return false;
        }

        // 차례가 된 손님만 통행 우선순위와 작은 반경을 적용해 군중 사이를 지나간다.
        return customers[0].MoveToCheckout(checkoutFront.position, frontCustomerRadius);
    }

}
