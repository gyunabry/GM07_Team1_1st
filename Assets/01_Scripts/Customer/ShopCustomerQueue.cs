using System.Collections.Generic;
using UnityEngine;

// 하나의 상점 계산대 앞 대기열과 각 손님의 NavMesh 목적지를 관리
public sealed class ShopCustomerQueue : MonoBehaviour
{
    [SerializeField] private Transform checkoutFront;
    [SerializeField, Min(0.1f)] private float spacing = 1.2f;
    [SerializeField] private int maxCustomers = 8;

    private readonly List<CustomerController> customers = new List<CustomerController>();

    public void Configure(Transform front, float slotSpacing, int capacity)
    {
        checkoutFront = front;
        spacing = Mathf.Max(0.1f, slotSpacing);
        maxCustomers = Mathf.Max(1, capacity);
    }

    public bool TryJoin(CustomerController customer)
    {
        if (customer == null || checkoutFront == null || customers.Contains(customer) || customers.Count >= maxCustomers)
        {
            return false;
        }

        customers.Add(customer);
        RefreshDestinations();
        return true;
    }

    public void Leave(CustomerController customer)
    {
        if (customer != null && customers.Remove(customer))
        {
            RefreshDestinations();
        }
    }

    public bool IsFront(CustomerController customer)
    {
        return customers.Count > 0 && customers[0] == customer;
    }

    private void RefreshDestinations()
    {
        for (int i = 0; i < customers.Count; i++)
        {
            CustomerController customer = customers[i];
            if (customer != null)
            {
                customer.SetQueueDestination(checkoutFront.position - checkoutFront.forward * (spacing * i));
            }
        }
    }
}
