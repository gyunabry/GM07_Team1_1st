using UnityEngine;

// 계산대 프리팹에 붙여 명시적으로 구성된 Customer 요소를 등록하는 구성 요소다.
public sealed class CustomerCheckoutStation : MonoBehaviour
{
    [Header("Prefab References")]
    [SerializeField] private ShopCustomerQueue queue;
    [SerializeField] private ShopCheckout checkout;

    private bool isOpen = true;
    private bool hasStarted;

    public ShopCustomerQueue Queue => queue;
    public ShopCheckout Checkout => checkout;
    public bool IsAvailable => isOpen && isActiveAndEnabled && queue != null && checkout != null;

    private void Start()
    {
        if (!HasRequiredReferences())
        {
            Debug.LogError("CustomerCheckoutStation requires explicitly assigned ShopCustomerQueue and ShopCheckout references. Check the checkout prefab setup.", this);
            enabled = false;
            return;
        }

        hasStarted = true;
        RegisterOrOpen();
    }

    private void OnEnable()
    {
        isOpen = true;
        if (hasStarted)
        {
            RegisterOrOpen();
        }
    }

    private void OnDisable()
    {
        if (CustomerSpawnManager.Instance != null)
        {
            CustomerSpawnManager.Instance.UnregisterStation(this);
        }
    }

    // 위치·회전 변경 또는 철거 전에 호출한다. 새 손님 배정을 닫고 기존 손님을 무보상 퇴장시킨다.
    public void CloseStation()
    {
        isOpen = false;
        CustomerSpawnManager.Instance?.CloseStation(this);
    }

    // 위치·회전 변경과 NavMesh 갱신 후 호출한다.
    public void OpenStation()
    {
        isOpen = true;
        RegisterOrOpen();
    }

    // 런타임 Factory 및 CustomerVisualTest처럼 Inspector를 거치지 않는 구성에서만 사용한다.
    public void ConfigureReferences(ShopCustomerQueue queueReference, ShopCheckout checkoutReference)
    {
        queue = queueReference;
        checkout = checkoutReference;
    }

    internal void SetOpen(bool value)
    {
        isOpen = value;
    }

    private void RegisterOrOpen()
    {
        if (isOpen && HasRequiredReferences() && CustomerSpawnManager.Instance != null)
        {
            queue.PrepareCrowdSlots();
            CustomerSpawnManager.Instance.RegisterStation(this);
        }
    }

    private bool HasRequiredReferences()
    {
        return queue != null && checkout != null;
    }

    // 컴포넌트를 프리팹에 처음 추가할 때 Inspector 참조를 편하게 채운다.
    private void Reset()
    {
        queue = GetComponent<ShopCustomerQueue>();
        checkout = GetComponentInChildren<ShopCheckout>(true);
    }
}
