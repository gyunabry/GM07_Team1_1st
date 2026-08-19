using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 운반 직원 건물에 나중에 부착할 직원 전용 연결 컴포넌트입니다.
/// 건물 시스템은 변경하지 않으며, EmployeeManager 프로필만 설정되면 자동 고용·풀링을 연결합니다.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(PlacedBuilding))]
public sealed class CarrierEmployeeBuildingController : MonoBehaviour
{
    [SerializeField] private CarrierWorker carrierEmployeePrefab;
    [SerializeField] private Transform homePoint;

    private readonly Dictionary<int, CarrierWorker> workers = new();
    private EmployeeManager employeeManager;
    private CarrierCommandService commandService;
    private PlacedBuilding placedBuilding;
    private ItemInventory materialStorage;
    private Transform materialStoragePoint;
    private bool registeredByThisComponent;

    private void Awake()
    {
        placedBuilding = GetComponent<PlacedBuilding>();
        if (homePoint == null)
        {
            homePoint = transform;
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
            Debug.LogError("CarrierEmployeeBuildingController requires an EmployeeManager in the scene.", this);
            enabled = false;
            return;
        }

        employeeManager.EmployeeHired += HandleEmployeeHired;
        employeeManager.EmployeeRemoved += HandleEmployeeRemoved;
        commandService = FindFirstObjectByType<CarrierCommandService>();
        if (commandService == null)
        {
            commandService = employeeManager.gameObject.AddComponent<CarrierCommandService>();
        }

        commandService.RegisterController(this);

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
            employeeManager.EmployeeHired -= HandleEmployeeHired;
            employeeManager.EmployeeRemoved -= HandleEmployeeRemoved;
        }

        commandService?.UnregisterController(this);
        commandService = null;
    }

    private void OnDestroy()
    {
        if (registeredByThisComponent && employeeManager != null && placedBuilding != null)
        {
            employeeManager.TryUnregisterBuilding(placedBuilding);
        }

        ReturnAllWorkers();
    }

    /// <summary>CarrierCommandService가 전송기 연동 값을 전달할 때만 호출합니다.</summary>
    internal void ConfigureLogisticsInternal(ItemInventory sharedMaterialStorage, Transform sharedMaterialStoragePoint)
    {
        materialStorage = sharedMaterialStorage;
        materialStoragePoint = sharedMaterialStoragePoint;

        foreach (CarrierWorker worker in workers.Values)
        {
            worker.ConfigureLogistics(materialStorage, materialStoragePoint);
        }
    }

    internal bool TryAssignCommandInternal(CarrierCommandType type, ProductionBuilding targetBuilding)
    {
        foreach (CarrierWorker worker in workers.Values)
        {
            if (worker.TryAssignCommand(type, targetBuilding))
            {
                return true;
            }
        }

        return false;
    }

    internal bool TryClearOneCommandInternal(CarrierCommandType type, ProductionBuilding targetBuilding)
    {
        foreach (CarrierWorker worker in workers.Values)
        {
            if (worker.HasCommand && worker.CurrentCommand.Type == type && worker.CurrentCommand.TargetBuilding == targetBuilding)
            {
                worker.ClearCommandFromUi();
                return true;
            }
        }

        return false;
    }

    internal int GetCommandCountInternal(CarrierCommandType type, ProductionBuilding targetBuilding)
    {
        int count = 0;
        foreach (CarrierWorker worker in workers.Values)
        {
            if (worker.HasCommand && worker.CurrentCommand.Type == type && worker.CurrentCommand.TargetBuilding == targetBuilding)
            {
                count++;
            }
        }

        return count;
    }

    internal int GetAvailableWorkerCountInternal()
    {
        int count = 0;
        foreach (CarrierWorker worker in workers.Values)
        {
            if (worker.IsAvailableForCommand)
            {
                count++;
            }
        }

        return count;
    }

    private void HandleConstructionCompleted(PlacedBuilding building)
    {
        if (building == placedBuilding && building.IsComplete)
        {
            RegisterBuilding();
        }
    }

    private void RegisterBuilding()
    {
        if (employeeManager == null || placedBuilding == null)
        {
            return;
        }

        registeredByThisComponent = employeeManager.TryRegisterBuilding(placedBuilding);
        RefreshExistingWorkers();
    }

    private void RefreshExistingWorkers()
    {
        if (!employeeManager.TryGetEmployees(placedBuilding, out IReadOnlyList<EmployeeRuntimeData> employees))
        {
            return;
        }

        for (int i = 0; i < employees.Count; i++)
        {
            CreateWorker( employees[i]);
        }
    }

    private void HandleEmployeeHired(EmployeeRuntimeData employee)
    {
        if (employee != null && employee.Role == EmployeeRole.Carrier && employee.AssignedBuilding == placedBuilding)
        {
            CreateWorker(employee);
        }
    }

    private void HandleEmployeeRemoved(EmployeeRuntimeData employee)
    {
        if (employee != null && workers.TryGetValue(employee.EmployeeId, out CarrierWorker worker))
        {
            ReturnWorker(employee.EmployeeId, worker);
        }
    }

    private void CreateWorker(EmployeeRuntimeData employee)
    {
        if (employee == null || employee.Role != EmployeeRole.Carrier || workers.ContainsKey(employee.EmployeeId) || carrierEmployeePrefab == null)
        {
            return;
        }

        PoolManager poolManager = PoolManager.Instance;
        if (poolManager == null)
        {
            Debug.LogError("CarrierEmployeeBuildingController requires PoolManager in the scene.", this);
            return;
        }

        CarrierWorker worker = poolManager.GetPool(carrierEmployeePrefab);
        if (worker == null)
        {
            return;
        }

        worker.transform.SetParent(null, true);
        worker.transform.position = homePoint.position;
        worker.transform.rotation = homePoint.rotation;
        worker.Initialize(employeeManager, employee, homePoint);
        worker.ConfigureLogistics(materialStorage, materialStoragePoint);
        workers.Add(employee.EmployeeId, worker);
    }

    private void ReturnAllWorkers()
    {
        foreach (CarrierWorker worker in workers.Values)
        {
            ReturnWorker(worker);
        }

        workers.Clear();
    }

    private void ReturnWorker(int employeeId, CarrierWorker worker)
    {
        workers.Remove(employeeId);
        ReturnWorker(worker);
    }

    private static void ReturnWorker(CarrierWorker worker)
    {
        if (worker == null)
        {
            return;
        }

        worker.ResetForPool();
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnPool(worker);
        }
    }
}
