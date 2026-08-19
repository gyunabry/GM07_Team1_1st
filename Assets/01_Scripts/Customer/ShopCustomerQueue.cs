using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// 동적으로 생성한 군집 대기 위치를 계산대별로 관리한다.
public sealed class ShopCustomerQueue : MonoBehaviour
{
    [SerializeField] private Transform checkoutFront;
    [SerializeField] private int maxCustomers = 20;
    [Header("Crowd Waiting Area")]
    [SerializeField, Range(1, 5)] private int crowdColumns = 3;
    [SerializeField, Min(0.1f)] private float minimumCrowdDepth = 4f;
    [SerializeField, Min(0.1f)] private float crowdSlotSpacing = 0.75f;
    [SerializeField, Min(0f)] private float crowdSlotJitter = 0.18f;
    [SerializeField, Min(0f)] private float checkoutClearance = 0.05f;
    [SerializeField, Min(0.1f)] private float displacedCustomerTolerance = 0.5f;
    [SerializeField, Min(0.1f)] private float displacementCheckInterval = 1f;
    [SerializeField, Min(0.1f)] private float crowdAgentRadius = 0.65f;
    [SerializeField, Min(0.1f)] private float frontCustomerRadius = 0.35f;
    [SerializeField, Min(0.1f)] private float checkoutAcceptanceRadius = 0.9f;

    private readonly List<CustomerController> customers = new List<CustomerController>();
    private readonly List<Vector3> crowdSlots = new List<Vector3>();
    private readonly Dictionary<CustomerController, Vector3> assignedSlots = new Dictionary<CustomerController, Vector3>();
    private bool isAcceptingCustomers = true;
    private float generatedCrowdDepth;
    private float generatedCrowdWidth;
    private float nextDisplacementCheckTime;

    public int Count => customers.Count;
    public int Capacity => maxCustomers;
    public bool IsAcceptingCustomers => isAcceptingCustomers;

    public void Configure(Transform front, float slotSpacing, int capacity)
    {
        checkoutFront = front;
        crowdSlotSpacing = Mathf.Max(0.1f, slotSpacing);
        crowdAgentRadius = Mathf.Max(0.1f, slotSpacing * 0.52f);
        maxCustomers = Mathf.Max(1, capacity);
    }

    // 계산대가 NavMesh 갱신 후 다시 열릴 때 호출한다.
    public void PrepareCrowdSlots()
    {
        if (customers.Count > 0)
        {
            return;
        }

        crowdSlots.Clear();
        assignedSlots.Clear();
        generatedCrowdDepth = 0f;
        generatedCrowdWidth = 0f;

        if (checkoutFront == null)
        {
            return;
        }

        // 대기 위치 간격은 Agent 지름보다 넓어야 회피 때문에 군집 영역 밖으로 밀리지 않는다.
        float effectiveSlotSpacing = Mathf.Max(crowdSlotSpacing, crowdAgentRadius * 2f + 0.1f);
        int columns = Mathf.Max(1, crowdColumns);
        int waitingCustomerCapacity = Mathf.Max(0, maxCustomers - 1);
        int rows = Mathf.Max(1, Mathf.CeilToInt(waitingCustomerCapacity / (float)columns));
        generatedCrowdWidth = (columns - 1) * effectiveSlotSpacing + crowdSlotJitter * 2f;
        generatedCrowdDepth = Mathf.Max(
            minimumCrowdDepth,
            checkoutClearance + (rows - 1) * effectiveSlotSpacing + crowdSlotJitter);
        System.Random random = new System.Random(GetInstanceID());

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                float horizontal = (column - (columns - 1) * 0.5f) * effectiveSlotSpacing;
                // 첫 행은 CheckoutFront와 거의 겹치게 시작해, 계산대 바로 앞의 밀집 군집을 만든다.
                // 첫 행은 CheckoutFront 바로 앞에서 시작하고, 이후 행만 대기 방향으로 확장한다.
                float depth = checkoutClearance + row * effectiveSlotSpacing;
                float jitterX = ((float)random.NextDouble() * 2f - 1f) * crowdSlotJitter;
                float jitterDepth = ((float)random.NextDouble() * 2f - 1f) * crowdSlotJitter;
                Vector3 candidate = checkoutFront.position
                    // CheckoutFront.forward는 계산대 쪽을 향하므로, 손님 군집은 반대 방향에 만든다.
                    - checkoutFront.forward * Mathf.Max(0f, depth + jitterDepth)
                    + checkoutFront.right * (horizontal + jitterX);

                // 장애물·카운터 가장자리에서도 충분한 후보를 확보한다. SamplePosition이 보정한
                // 최종 위치는 바로 아래 IsInsideCrowdArea로 계산대 앞 영역 안인지 검증한다.
                float sampleRadius = effectiveSlotSpacing * 0.6f;
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas)
                    && IsInsideCrowdArea(hit.position)
                    && IsNewSlot(hit.position, effectiveSlotSpacing))
                {
                    crowdSlots.Add(hit.position);
                }
            }
        }
    }

    public bool TryJoin(CustomerController customer)
    {
        if (!isAcceptingCustomers || customer == null || checkoutFront == null || customers.Contains(customer) || customers.Count >= maxCustomers)
        {
            return false;
        }

        PrepareCrowdSlots();
        customers.Add(customer);

        if (customers.Count == 1)
        {
            return customer.MoveToCheckout(checkoutFront.position, frontCustomerRadius);
        }

        if (!TryAssignCrowdSlot(customer, out Vector3 destination))
        {
            customers.Remove(customer);
            Debug.LogWarning("Not enough valid crowd waiting positions on the NavMesh for this checkout.", this);
            return false;
        }

        customer.SetNavigationRadius(crowdAgentRadius);
        return customer.QueueMovement != null && customer.QueueMovement.SetDestination(destination);
    }

    public void Leave(CustomerController customer)
    {
        if (customer == null || !customers.Remove(customer))
        {
            return;
        }

        assignedSlots.Remove(customer);
        if (customers.Count == 0)
        {
            return;
        }

        // 새 맨 앞 손님만 자기 군집 위치를 비우고 계산대로 이동한다.
        CustomerController nextCustomer = customers[0];
        assignedSlots.Remove(nextCustomer);
        MoveFrontCustomerToCheckout();
        ReassignWaitingCustomers();
    }

    public bool IsFront(CustomerController customer)
    {
        return customers.Count > 0 && customers[0] == customer;
    }

    public void SetAcceptingCustomers(bool value)
    {
        isAcceptingCustomers = value;
    }

    public CustomerController[] GetCustomersSnapshot()
    {
        return customers.ToArray();
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

        return customers[0].MoveToCheckout(checkoutFront.position, frontCustomerRadius);
    }

    private void Update()
    {
        if (Time.time < nextDisplacementCheckTime)
        {
            return;
        }

        nextDisplacementCheckTime = Time.time + displacementCheckInterval;
        for (int i = 1; i < customers.Count; i++)
        {
            CustomerController customer = customers[i];
            if (customer != null && customer.QueueMovement != null && customer.QueueMovement.IsOutsideDestination(displacedCustomerTolerance))
            {
                // 다른 계산대의 퇴장 손님에게 밀린 경우에만 기존 목적지 경로를 재개한다.
                // 목적지가 같으면 CustomerController의 캐시가 새 CalculatePath를 생략한다.
                customer.QueueMovement.ResumeMovement();
            }
        }
    }

    private bool TryAssignCrowdSlot(CustomerController customer, out Vector3 destination)
    {
        destination = default;
        float bestScore = float.MaxValue;

        for (int i = 0; i < crowdSlots.Count; i++)
        {
            Vector3 slot = crowdSlots[i];
            if (IsSlotAssigned(slot))
            {
                continue;
            }

            // 입구에서 가까운 자리가 아니라 계산대에서 가까운 행부터 채운다.
            // 그래야 손님 수가 적을 때도 군집이 CheckoutFront 바로 앞에 형성된다.
            Vector3 offset = slot - checkoutFront.position;
            offset.y = 0f;
            float queueDepth = -Vector3.Dot(offset, checkoutFront.forward);
            float sidewaysDistance = Mathf.Abs(Vector3.Dot(offset, checkoutFront.right));
            float score = queueDepth * 100f + sidewaysDistance;
            if (score < bestScore)
            {
                bestScore = score;
                destination = slot;
            }
        }

        if (bestScore == float.MaxValue)
        {
            return false;
        }

        assignedSlots.Add(customer, destination);
        return true;
    }

    // 결제·퇴장으로 앞쪽 자리가 비면, 남은 군집을 계산대 가까운 후보부터 다시 채운다.
    // 이 작업은 손님이 떠나는 순간에만 실행되므로 지속적인 군중 회피 갱신을 만들지 않는다.
    private void ReassignWaitingCustomers()
    {
        assignedSlots.Clear();

        for (int i = 1; i < customers.Count; i++)
        {
            CustomerController customer = customers[i];
            if (customer == null)
            {
                continue;
            }

            if (!TryAssignCrowdSlot(customer, out Vector3 destination))
            {
                Debug.LogWarning("Not enough valid crowd waiting positions while compacting this checkout queue.", this);
                continue;
            }

            customer.SetNavigationRadius(crowdAgentRadius);
            customer.QueueMovement?.SetDestination(destination);
        }
    }

    private bool IsSlotAssigned(Vector3 slot)
    {
        foreach (Vector3 assignedSlot in assignedSlots.Values)
        {
            if ((assignedSlot - slot).sqrMagnitude <= 0.0001f)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsNewSlot(Vector3 candidate, float slotSpacing)
    {
        float minimumDistanceSquared = slotSpacing * slotSpacing * 0.64f;
        for (int i = 0; i < crowdSlots.Count; i++)
        {
            if ((crowdSlots[i] - candidate).sqrMagnitude < minimumDistanceSquared)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsInsideCrowdArea(Vector3 position)
    {
        Vector3 offset = position - checkoutFront.position;
        offset.y = 0f;

        float queueDepth = -Vector3.Dot(offset, checkoutFront.forward);
        float sidewaysDistance = Mathf.Abs(Vector3.Dot(offset, checkoutFront.right));
        return queueDepth >= -0.01f
            && queueDepth <= generatedCrowdDepth
            && sidewaysDistance <= generatedCrowdWidth * 0.5f;
    }
}
