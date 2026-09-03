using System;
using System.Collections.Generic;
using UnityEngine;

// 계산대 담당자 감지 트리거. 결제 판단과 실제 결제는 손님이 담당한다.
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public sealed class ShopCheckout : MonoBehaviour
{
    private readonly Dictionary<IShopCheckoutOperator, int> operatorColliderCounts = new Dictionary<IShopCheckoutOperator, int>();
    private float paymentDurationReductionPercent;

    public event Action OperatorPresenceChanged;
    public event Action<float> PaymentProgressChanged;
    public event Action PaymentCompleted;

    public bool HasOperator => operatorColliderCounts.Count > 0;
    public float PaymentDurationMultiplier => 1f - paymentDurationReductionPercent / 100f;

    public void SetPaymentDurationReductionPercent(float percent)
    {
        paymentDurationReductionPercent = Mathf.Clamp(percent, 0f, 100f);
    }

    public void SetPaymentProgress(float normalizedProgress)
    {
        PaymentProgressChanged?.Invoke(Mathf.Clamp01(normalizedProgress));
    }

    public void ClearPaymentProgress()
    {
        PaymentProgressChanged?.Invoke(-1f);
    }

    public void NotifyPaymentCompleted()
    {
        PaymentCompleted?.Invoke();
    }

    private void Awake()
    {
        BoxCollider trigger = GetComponent<BoxCollider>();
        trigger.isTrigger = true;

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;
    }

    public void ConfigureZone(Vector3 size)
    {
        GetComponent<BoxCollider>().size = size;
    }

    private void OnTriggerEnter(Collider other)
    {
        IShopCheckoutOperator checkoutOperator = FindOperator(other);
        if (checkoutOperator == null)
        {
            return;
        }

        bool wasEmpty = operatorColliderCounts.Count == 0;
        if (operatorColliderCounts.TryGetValue(checkoutOperator, out int count))
        {
            operatorColliderCounts[checkoutOperator] = count + 1;
        }
        else
        {
            operatorColliderCounts.Add(checkoutOperator, 1);
        }

        if (wasEmpty)
        {
            OperatorPresenceChanged?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IShopCheckoutOperator checkoutOperator = FindOperator(other);
        if (checkoutOperator == null || !operatorColliderCounts.TryGetValue(checkoutOperator, out int count))
        {
            return;
        }

        if (count > 1)
        {
            operatorColliderCounts[checkoutOperator] = count - 1;
            return;
        }

        operatorColliderCounts.Remove(checkoutOperator);
        if (operatorColliderCounts.Count == 0)
        {
            OperatorPresenceChanged?.Invoke();
        }
    }

    private static IShopCheckoutOperator FindOperator(Component component)
    {
        MonoBehaviour[] behaviours = component.GetComponentsInParent<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IShopCheckoutOperator checkoutOperator)
            {
                return checkoutOperator;
            }
        }

        return null;
    }
}
