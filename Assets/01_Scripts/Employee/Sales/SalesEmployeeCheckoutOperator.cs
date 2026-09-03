using UnityEngine;

/// <summary>
/// 판매대에 고용된 판매 직원이 존재하는 동안 계산 담당자 표식을 유지합니다.
/// 직원 프리팹이 준비되기 전까지는 보이지 않는 Collider만 생성하며,
/// 이후 시각 프리팹 생성 로직으로 교체할 수 있습니다.
/// </summary>
[DisallowMultipleComponent]
public sealed class SalesEmployeeCheckoutOperator : MonoBehaviour
{
    private const string OperatorObjectName = "SalesEmployeeOperator";

    [SerializeField] private GameObject salesEmployeePrefab;
    [SerializeField] private Transform spawnPoint;

    private EmployeeManager employeeManager;
    private PlacedBuilding placedBuilding;
    private ShopCheckout checkout;
    private GameObject operatorObject;
    private EmployeeWorkProgressHUD operatorProgressHud;
    private SalesEmployeeAnimationController operatorAnimationController;
    private bool registeredByThisComponent;

    private void Awake()
    {
        placedBuilding = GetComponent<PlacedBuilding>();
        checkout = GetComponentInChildren<ShopCheckout>(true);

        if (placedBuilding == null || checkout == null)
        {
            Debug.LogError("SalesEmployeeCheckoutOperator requires PlacedBuilding and ShopCheckout on the sales counter.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (placedBuilding != null)
        {
            placedBuilding.OnConstructionCompleted += HandleConstructionCompleted;
        }
    }

    private void Start()
    {
        employeeManager = FindFirstObjectByType<EmployeeManager>();
        if (employeeManager == null)
        {
            Debug.LogError("SalesEmployeeCheckoutOperator requires an EmployeeManager in the scene.", this);
            enabled = false;
            return;
        }

        employeeManager.EmployeeHired += HandleEmployeeChanged;
        employeeManager.EmployeeRemoved += HandleEmployeeChanged;
        employeeManager.SalesPaymentTimeReductionChanged += ApplySalesPaymentTimeReduction;
        employeeManager.AllEmployeeProcessingSpeedIncreaseChanged += ApplyAllEmployeeProcessingSpeedIncrease;
        checkout.PaymentProgressChanged += HandlePaymentProgressChanged;
        checkout.PaymentCompleted += HandlePaymentCompleted;
        RefreshPaymentDuration();

        if (placedBuilding.IsComplete)
        {
            RegisterBuilding();
        }
    }

    private void OnDisable()
    {
        if (placedBuilding != null)
        {
            placedBuilding.OnConstructionCompleted -= HandleConstructionCompleted;
        }

        if (employeeManager != null)
        {
            employeeManager.EmployeeHired -= HandleEmployeeChanged;
            employeeManager.EmployeeRemoved -= HandleEmployeeChanged;
            employeeManager.SalesPaymentTimeReductionChanged -= ApplySalesPaymentTimeReduction;
            employeeManager.AllEmployeeProcessingSpeedIncreaseChanged -= ApplyAllEmployeeProcessingSpeedIncrease;
        }

        if (checkout != null)
        {
            checkout.PaymentProgressChanged -= HandlePaymentProgressChanged;
            checkout.PaymentCompleted -= HandlePaymentCompleted;
        }

        RemoveOperator();
    }

    private void OnDestroy()
    {
        if (registeredByThisComponent && employeeManager != null && placedBuilding != null)
        {
            employeeManager.TryUnregisterBuilding(placedBuilding);
        }
    }

    private void HandleConstructionCompleted(PlacedBuilding building)
    {
        if (building == placedBuilding && building.IsComplete)
        {
            RegisterBuilding();
        }
    }

    private void HandleEmployeeChanged(EmployeeRuntimeData employee)
    {
        RefreshOperator();
    }

    private void ApplySalesPaymentTimeReduction(float _)
    {
        RefreshPaymentDuration();
    }

    private void ApplyAllEmployeeProcessingSpeedIncrease(float _)
    {
        RefreshPaymentDuration();
    }

    private void RefreshPaymentDuration()
    {
        if (employeeManager == null)
        {
            return;
        }

        float totalReductionPercent = Mathf.Clamp(
            employeeManager.SalesPaymentTimeReductionPercent + employeeManager.AllEmployeeProcessingSpeedIncreasePercent,
            0f,
            100f);
        checkout?.SetPaymentDurationReductionPercent(totalReductionPercent);
    }

    private void RegisterBuilding()
    {
        if (employeeManager == null || placedBuilding == null)
        {
            return;
        }

        registeredByThisComponent = employeeManager.TryRegisterSalesBuilding(placedBuilding);
        RefreshOperator();
    }

    private void RefreshOperator()
    {
        if (employeeManager == null || placedBuilding == null || checkout == null)
        {
            RemoveOperator();
            return;
        }

        if (!employeeManager.TryGetEmployees(placedBuilding, out System.Collections.Generic.IReadOnlyList<EmployeeRuntimeData> employees))
        {
            RemoveOperator();
            return;
        }

        for (int i = 0; i < employees.Count; i++)
        {
            EmployeeRuntimeData employee = employees[i];
            if (employee != null && employee.Role == EmployeeRole.Sales)
            {
                employeeManager.TrySetWorkState(employee, EmployeeWorkState.Working);
                EnsureOperator();
                return;
            }
        }

        RemoveOperator();
    }

    private void EnsureOperator()
    {
        if (operatorObject != null)
        {
            return;
        }

        if (salesEmployeePrefab == null)
        {
            Debug.LogError("SalesEmployeeCheckoutOperator requires a Sales Employee Prefab reference.", this);
            return;
        }

        Vector3 position = GetOperatorPosition();
        operatorObject = Instantiate(salesEmployeePrefab, spawnPoint.position, spawnPoint.rotation, transform);
        operatorObject.name = OperatorObjectName;

        EnsureCheckoutOperatorComponents(operatorObject);
        operatorProgressHud = operatorObject.GetComponent<EmployeeWorkProgressHUD>();
        operatorAnimationController = operatorObject.GetComponent<SalesEmployeeAnimationController>();
        if (operatorAnimationController == null)
        {
            operatorAnimationController = operatorObject.AddComponent<SalesEmployeeAnimationController>();
        }
    }

    private Vector3 GetOperatorPosition()
    {
        BoxCollider checkoutCollider = checkout.GetComponent<BoxCollider>();
        return checkoutCollider != null
            ? checkout.transform.TransformPoint(checkoutCollider.center)
            : checkout.transform.position;
    }

    private static void EnsureCheckoutOperatorComponents(GameObject employeeObject)
    {
        if (employeeObject.GetComponent<CheckoutOperatorPresence>() == null)
        {
            employeeObject.AddComponent<CheckoutOperatorPresence>();
        }

        if (employeeObject.GetComponent<BoxCollider>() == null)
        {
            BoxCollider collider = employeeObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.5f, 1.8f, 0.5f);
            collider.center = new Vector3(0f, 0.9f, 0f);
        }
    }

    private void RemoveOperator()
    {
        if (operatorObject == null)
        {
            return;
        }

        Destroy(operatorObject);
        operatorObject = null;
        operatorProgressHud = null;
        operatorAnimationController = null;
    }

    private void HandlePaymentProgressChanged(float normalizedProgress)
    {
        operatorAnimationController?.SetSelling(normalizedProgress >= 0f);

        if (operatorProgressHud == null)
        {
            return;
        }

        if (normalizedProgress < 0f)
        {
            operatorProgressHud.Hide();
            return;
        }

        operatorProgressHud.ShowProgress(normalizedProgress);
    }

    private void HandlePaymentCompleted()
    {
        operatorAnimationController?.PlaySaleCompleted();
    }
}
