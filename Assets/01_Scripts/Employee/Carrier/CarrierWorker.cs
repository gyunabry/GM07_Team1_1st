using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 운반 직원 한 명의 시각 오브젝트와 업무 실행을 담당합니다.
/// 전송기 인벤토리·작업 위치는 외부 시스템이 ConfigureLogistics로 주입합니다.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
public sealed class CarrierWorker : MonoBehaviour
{
    private const float NavMeshSampleDistance = 3f;
    private const float DestinationRepathDistance = 0.05f;

    private enum TaskState
    {
        Idle,
        MoveToSource,
        WaitAtSource,
        MoveToDestination,
        WaitAtDestination,
        ReturnToMaterialStorage,
        ReturnHome
    }

    private enum ClearReason
    {
        Manual,
        RecipeChanged
    }

    [SerializeField, Min(0.1f)] private float movementSpeed = 3.5f;
    [SerializeField, Min(1)] private int carryingCapacity = 5;
    [SerializeField, Min(0.05f)] private float stoppingDistance = 1.5f;
    [SerializeField, Min(0.05f)] private float pickupDuration = 10f;
    [SerializeField, Min(0.05f)] private float deliveryDuration = 10f;
    [SerializeField] private ItemInventory cargoInventory = new();

    private NavMeshAgent agent;
    private EmployeeManager employeeManager;
    private EmployeeRuntimeData employee;
    private Transform homePoint;
    private ItemInventory materialStorage;
    private Transform materialStoragePoint;
    private CarrierCommand command;
    private ItemInventory commandClearDestination;
    private Transform commandClearDestinationPoint;
    private Transform activeDestination;
    private TaskState taskState;
    private bool hasCommand;
    private bool isInitialized;
    private float pickupElapsed;
    private float deliveryElapsed;
    private float pickupTimeReductionPercent;
    private float deliveryTimeReductionPercent;
    private float baseMovementSpeed;
    private int baseCarryingCapacity;
    private int skillCarryingCapacityBonus;
    private float movementSpeedIncreasePercent;
    private EmployeeWorkProgressHUD workProgressHud;

    public EmployeeRuntimeData Employee => employee;
    public ItemInventory CargoInventory => cargoInventory;
    public float MovementSpeed => movementSpeed;
    public int CarryingCapacity => carryingCapacity;
    public bool HasCommand => hasCommand;
    public bool IsAvailableForCommand => isInitialized && !hasCommand && taskState == TaskState.Idle;
    public CarrierCommand CurrentCommand => command;

    public event Action<CarrierWorker> BecameAvailable;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (GetComponent<CarrierVisualMotionController>() == null)
        {
            gameObject.AddComponent<CarrierVisualMotionController>();
        }

        workProgressHud = GetComponent<EmployeeWorkProgressHUD>();
        cargoInventory.InventoryChanged += RefreshCargoHud;
        baseMovementSpeed = movementSpeed;
        baseCarryingCapacity = carryingCapacity;
        ApplyCarryingCapacity();
    }

    private void OnDisable()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }
    }

    private void Update()
    {
        if (!isInitialized)
        {
            return;
        }

        if (!hasCommand)
        {
            UpdateWithoutCommand();
            return;
        }

        if (!command.IsValid || command.TargetBuilding.SelectedRecipe != command.AssignedRecipe)
        {
            ClearCommand(ClearReason.RecipeChanged);
            return;
        }

        RefreshPathForMovedDestination();

        switch (taskState)
        {
            case TaskState.Idle:
                BeginCommandCycle();
                break;
            case TaskState.MoveToSource:
                if (HasArrived())
                {
                    taskState = TaskState.WaitAtSource;
                    pickupElapsed = 0f;
                    ProcessSource();
                }
                break;
            case TaskState.WaitAtSource:
                ProcessSource();
                break;
            case TaskState.MoveToDestination:
                if (HasArrived())
                {
                    taskState = TaskState.WaitAtDestination;
                    deliveryElapsed = 0f;
                    ProcessDestination();
                }
                break;
            case TaskState.WaitAtDestination:
                ProcessDestination();
                break;
            case TaskState.ReturnToMaterialStorage:
                if (HasArrived())
                {
                    ReturnCargoToMaterialStorage();
                }
                break;
            case TaskState.ReturnHome:
                if (HasArrived())
                {
                    FinishReturnHome();
                }
                break;
        }
    }

    /// <summary>풀에서 대여된 직원을 고용 데이터와 대기 위치에 연결합니다.</summary>
    public void Initialize(EmployeeManager manager, EmployeeRuntimeData runtimeEmployee, Transform assignedHomePoint)
    {
        employeeManager = manager;
        employee = runtimeEmployee;
        homePoint = assignedHomePoint;
        command = default;
        commandClearDestination = null;
        commandClearDestinationPoint = null;
        activeDestination = null;
        hasCommand = false;
        taskState = TaskState.Idle;
        isInitialized = employee != null;
        pickupElapsed = 0f;
        deliveryElapsed = 0f;

        if (agent != null)
        {
            agent.speed = movementSpeed;
            agent.stoppingDistance = stoppingDistance;
            agent.isStopped = false;
        }

        EnterDormant();
        RefreshCargoHud();
    }

    /// <summary>
    /// 상점 통합 전송기 담당 시스템이 제공해야 하는 인벤토리와 작업 위치를 연결합니다.
    /// </summary>
    public void ConfigureLogistics(ItemInventory sharedMaterialStorage, Transform sharedMaterialStoragePoint)
    {
        materialStorage = sharedMaterialStorage;
        materialStoragePoint = sharedMaterialStoragePoint;
    }

    public void SetTransferTimeReductionPercents(float pickupReductionPercent, float deliveryReductionPercent)
    {
        pickupTimeReductionPercent = Mathf.Clamp(pickupReductionPercent, 0f, 100f);
        deliveryTimeReductionPercent = Mathf.Clamp(deliveryReductionPercent, 0f, 100f);
    }

    public void SetMovementSpeedIncreasePercent(float percent)
    {
        movementSpeedIncreasePercent = Mathf.Max(0f, percent);
        ApplyMovementSpeed();
    }

    public void SetSkillCarryingCapacityBonus(int amount)
    {
        skillCarryingCapacityBonus = Mathf.Max(0, amount);
        ApplyCarryingCapacity();
    }

    private void ApplyCarryingCapacity()
    {
        carryingCapacity = Mathf.Max(1, baseCarryingCapacity + skillCarryingCapacityBonus);
        cargoInventory.SetBaseCapacity(carryingCapacity);
        RefreshCargoHud();
    }

    private void ApplyMovementSpeed()
    {
        movementSpeed = Mathf.Max(0.1f, baseMovementSpeed * (1f + movementSpeedIncreasePercent / 100f));

        if (agent != null)
        {
            agent.speed = movementSpeed;
        }
    }

    public bool TryAssignCommand(CarrierCommandType type, ProductionBuilding targetBuilding)
    {
        if (!IsAvailableForCommand || targetBuilding == null || targetBuilding.SelectedRecipe == null)
        {
            return false;
        }

        command = new CarrierCommand(type, targetBuilding);
        hasCommand = command.IsValid;
        if (!hasCommand)
        {
            return false;
        }

        ActivateAtHome();
        BeginCommandCycle();
        return true;
    }

    /// <summary>직원 UI의 - 버튼 또는 대상 건물 생산품 변경 시 호출합니다.</summary>
    public void ClearCommandFromUi()
    {
        ClearCommand(ClearReason.Manual);
    }

    /// <summary>풀 반환 전에 호출해 직원별 런타임 상태를 완전히 초기화합니다.</summary>
    public void ResetForPool()
    {
        StopAgent();
        employee = null;
        employeeManager = null;
        homePoint = null;
        materialStorage = null;
        materialStoragePoint = null;
        command = default;
        commandClearDestination = null;
        commandClearDestinationPoint = null;
        activeDestination = null;
        hasCommand = false;
        isInitialized = false;
        taskState = TaskState.Idle;
        pickupElapsed = 0f;
        deliveryElapsed = 0f;
        RefreshCargoHud();
    }

    private void BeginCommandCycle()
    {
        if (!hasCommand || !command.IsValid)
        {
            return;
        }

        if (command.Type == CarrierCommandType.Material)
        {
            MoveTo(materialStoragePoint, TaskState.MoveToSource);
            return;
        }

        MoveTo(command.TargetBuilding.transform, TaskState.MoveToSource);
    }

    private void ProcessSource()
    {
        ItemDataSO itemToPickUp = command.Type == CarrierCommandType.Material
            ? command.AssignedRecipe.Input
            : command.AssignedRecipe.Output;
        ItemInventory sourceInventory = command.Type == CarrierCommandType.Material
            ? materialStorage
            : command.TargetBuilding.OutputInventory;

        if (itemToPickUp == null || sourceInventory == null || sourceInventory.GetAmount(itemToPickUp) <= 0)
        {
            pickupElapsed = 0f;
            SetWorkingState(EmployeeWorkState.Idle);
            return;
        }

        if (!TryCompletePickup())
        {
            return;
        }

        if (command.Type == CarrierCommandType.Material)
        {
            ItemDataSO inputItem = command.AssignedRecipe.Input;
            int moved = TransferUpToCapacity(materialStorage, cargoInventory, inputItem);
            if (moved <= 0)
            {
                pickupElapsed = 0f;
                SetWorkingState(EmployeeWorkState.Idle);
                return;
            }

            pickupElapsed = 0f;
            MoveTo(command.TargetBuilding.transform, TaskState.MoveToDestination);
            return;
        }

        ItemDataSO outputItem = command.AssignedRecipe.Output;
        int received = TransferUpToCapacity(command.TargetBuilding.OutputInventory, cargoInventory, outputItem);
        if (received <= 0)
        {
            pickupElapsed = 0f;
            SetWorkingState(EmployeeWorkState.Idle);
            return;
        }

        SalesCounter counter = FindNearestSalesCounter();
        if (counter == null)
        {
            taskState = TaskState.WaitAtSource;
            SetWorkingState(EmployeeWorkState.Idle);
            return;
        }

        pickupElapsed = 0f;
        MoveTo(counter.transform, TaskState.MoveToDestination);
    }

    private void ProcessDestination()
    {
        if (!TryCompleteDelivery())
        {
            return;
        }

        ItemDataSO item = command.Type == CarrierCommandType.Material
            ? command.AssignedRecipe.Input
            : command.AssignedRecipe.Output;
        ItemInventory destination = command.Type == CarrierCommandType.Material
            ? command.TargetBuilding.InputInventory
            : FindNearestSalesCounter()?.Inventory;

        if (destination == null || item == null)
        {
            deliveryElapsed = 0f;
            SetWorkingState(EmployeeWorkState.Idle);
            return;
        }

        int moved = cargoInventory.TransferTo(destination, item, cargoInventory.GetAmount(item));
        if (moved <= 0 && cargoInventory.GetAmount(item) > 0)
        {
            deliveryElapsed = 0f;
            SetWorkingState(EmployeeWorkState.Idle);
            return;
        }

        deliveryElapsed = 0f;
        taskState = TaskState.Idle;
        SetWorkingState(EmployeeWorkState.Idle);
    }

    private void ClearCommand(ClearReason reason)
    {
        CarrierCommand previousCommand = command;
        hasCommand = false;
        command = default;

        if (cargoInventory.TotalAmount <= 0)
        {
            MoveToHome();
            return;
        }

        if (reason == ClearReason.RecipeChanged && previousCommand.Type == CarrierCommandType.Material)
        {
            SetCommandClearDestination(materialStorage, materialStoragePoint);
            MoveTo(materialStoragePoint, TaskState.MoveToDestination);
            return;
        }

        if (previousCommand.Type == CarrierCommandType.Material)
        {
            ItemInventory destination = previousCommand.TargetBuilding != null ? previousCommand.TargetBuilding.InputInventory : null;
            Transform destinationPoint = previousCommand.TargetBuilding != null ? previousCommand.TargetBuilding.transform : null;
            SetCommandClearDestination(destination, destinationPoint);
            MoveTo(destinationPoint, TaskState.MoveToDestination);
            return;
        }

        SalesCounter counter = FindNearestSalesCounter();
        if (counter != null)
        {
            SetCommandClearDestination(counter.Inventory, counter.transform);
            MoveTo(counter.transform, TaskState.MoveToDestination);
            return;
        }

        MoveToHome();
    }

    private void UpdateWithoutCommand()
    {
        RefreshPathForMovedDestination();

        if (taskState == TaskState.MoveToDestination || taskState == TaskState.WaitAtDestination)
        {
            DeliverCargoAfterCommandClear();
            return;
        }

        if (taskState == TaskState.ReturnToMaterialStorage)
        {
            if (HasArrived())
            {
                ReturnCargoToMaterialStorage();
            }
            return;
        }

        if (taskState == TaskState.ReturnHome && HasArrived())
        {
            FinishReturnHome();
        }
    }

    private void DeliverCargoAfterCommandClear()
    {
        if (taskState == TaskState.MoveToDestination && !HasArrived())
        {
            return;
        }

        // 명령 해제 시에는 기존 목적지에 소지품을 모두 납품한 뒤 복귀합니다.
        if (commandClearDestination == null)
        {
            SetWorkingState(EmployeeWorkState.Idle);
            return;
        }

        TransferAllCargo(commandClearDestination);
        if (cargoInventory.TotalAmount == 0)
        {
            commandClearDestination = null;
            commandClearDestinationPoint = null;
            MoveToHome();
        }
    }

    private void ReturnCargoToMaterialStorage()
    {
        if (materialStorage == null)
        {
            SetWorkingState(EmployeeWorkState.Idle);
            return;
        }

        TransferAllCargo(materialStorage);
        if (cargoInventory.TotalAmount == 0)
        {
            MoveToHome();
        }
    }

    private void MoveToHome()
    {
        if (!MoveTo(homePoint, TaskState.ReturnHome))
        {
            FinishReturnHome();
        }
    }

    private void FinishReturnHome()
    {
        taskState = TaskState.Idle;
        activeDestination = null;
        SetWorkingState(EmployeeWorkState.Idle);
        BecameAvailable?.Invoke(this);
        EnterDormant();
    }

    private bool MoveTo(Transform destination, TaskState nextState)
    {
        if (destination == null || !EnsureAgentOnNavMesh())
        {
            taskState = TaskState.Idle;
            SetWorkingState(EmployeeWorkState.Idle);
            return false;
        }

        Vector3 approachPosition = GetApproachPosition(destination);
        if (!NavMesh.SamplePosition(approachPosition, out NavMeshHit destinationHit, NavMeshSampleDistance, agent.areaMask))
        {
            taskState = TaskState.Idle;
            SetWorkingState(EmployeeWorkState.Idle);
            return false;
        }

        NavMeshPath path = new();
        if (!agent.CalculatePath(destinationHit.position, path) || path.status != NavMeshPathStatus.PathComplete || !agent.SetDestination(destinationHit.position))
        {
            taskState = TaskState.Idle;
            SetWorkingState(EmployeeWorkState.Idle);
            return false;
        }

        taskState = nextState;
        activeDestination = destination;
        agent.isStopped = false;
        SetWorkingState(EmployeeWorkState.Moving);
        return true;
    }

    private bool HasArrived()
    {
        return agent != null && agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    private int TransferUpToCapacity(ItemInventory source, ItemInventory target, ItemDataSO item)
    {
        if (source == null || target == null || item == null)
        {
            return 0;
        }

        int remainingCarry = Mathf.Max(0, carryingCapacity - target.TotalAmount);
        return source.TransferTo(target, item, remainingCarry);
    }

    private bool TryCompletePickup()
    {
        SetWorkingState(EmployeeWorkState.Working);
        pickupElapsed += Time.deltaTime;
        float duration = Mathf.Max(0.05f, pickupDuration * (1f - pickupTimeReductionPercent / 100f));
        workProgressHud?.ShowProgress(pickupElapsed / duration, cargoInventory);
        return pickupElapsed >= duration;
    }

    private bool TryCompleteDelivery()
    {
        SetWorkingState(EmployeeWorkState.Working);
        deliveryElapsed += Time.deltaTime;
        float duration = Mathf.Max(0.05f, deliveryDuration * (1f - deliveryTimeReductionPercent / 100f));
        workProgressHud?.ShowProgress(deliveryElapsed / duration, cargoInventory);
        return deliveryElapsed >= duration;
    }

    private void TransferAllCargo(ItemInventory destination)
    {
        for (int i = cargoInventory.Entries.Count - 1; i >= 0; i--)
        {
            InventoryEntry entry = cargoInventory.Entries[i];
            if (entry != null && !entry.IsEmpty)
            {
                cargoInventory.TransferTo(destination, entry.Item, entry.Amount);
            }
        }
    }

    private SalesCounter FindNearestSalesCounter()
    {
        SalesCounter[] counters = FindObjectsByType<SalesCounter>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        SalesCounter nearest = null;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < counters.Length; i++)
        {
            SalesCounter candidate = counters[i];
            if (candidate == null || !candidate.CanOperate || candidate.Inventory == null)
            {
                continue;
            }

            float distance = (candidate.transform.position - transform.position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = candidate;
            }
        }

        return nearest;
    }

    private void SetWorkingState(EmployeeWorkState state)
    {
        if (state != EmployeeWorkState.Working)
        {
            workProgressHud?.Hide();
        }

        employeeManager?.TrySetWorkState(employee, state);
    }

    private void OnDestroy()
    {
        cargoInventory.InventoryChanged -= RefreshCargoHud;
    }

    private void RefreshCargoHud()
    {
        workProgressHud?.RefreshCargo(cargoInventory);
    }

    private void SetCommandClearDestination(ItemInventory destination, Transform destinationPoint)
    {
        commandClearDestination = destination;
        commandClearDestinationPoint = destinationPoint;
    }

    private void StopAgent()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }
    }

    private void ActivateAtHome()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (homePoint != null && agent != null)
        {
            transform.SetPositionAndRotation(homePoint.position, homePoint.rotation);
            EnsureAgentOnNavMesh();
        }

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
    }

    private void EnterDormant()
    {
        StopAgent();
        taskState = TaskState.Idle;
        activeDestination = null;
        SetWorkingState(EmployeeWorkState.Idle);

        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    private bool EnsureAgentOnNavMesh()
    {
        if (agent == null)
        {
            return false;
        }

        if (!agent.enabled)
        {
            agent.enabled = true;
        }

        if (agent.isOnNavMesh)
        {
            return true;
        }

        if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, NavMeshSampleDistance, agent.areaMask))
        {
            return false;
        }

        return agent.Warp(hit.position);
    }

    private Vector3 GetApproachPosition(Transform destination)
    {
        Collider[] colliders = destination.GetComponentsInChildren<Collider>(true);
        Vector3 closestPoint = destination.position;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == null || !collider.enabled)
            {
                continue;
            }

            Vector3 candidate = collider.ClosestPoint(transform.position);
            float distance = (candidate - transform.position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = candidate;
            }
        }

        return closestPoint;
    }

    private void RefreshPathForMovedDestination()
    {
        if (activeDestination == null || agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            return;
        }

        TaskState moveState;
        switch (taskState)
        {
            case TaskState.MoveToSource:
            case TaskState.WaitAtSource:
                moveState = TaskState.MoveToSource;
                break;
            case TaskState.MoveToDestination:
            case TaskState.WaitAtDestination:
                moveState = TaskState.MoveToDestination;
                break;
            case TaskState.ReturnToMaterialStorage:
                moveState = TaskState.ReturnToMaterialStorage;
                break;
            case TaskState.ReturnHome:
                moveState = TaskState.ReturnHome;
                break;
            default:
                moveState = TaskState.Idle;
                break;
        }

        if (moveState == TaskState.Idle)
        {
            return;
        }

        Vector3 approachPosition = GetApproachPosition(activeDestination);
        if (!NavMesh.SamplePosition(approachPosition, out NavMeshHit destinationHit, NavMeshSampleDistance, agent.areaMask) ||
            (agent.destination - destinationHit.position).sqrMagnitude <= DestinationRepathDistance * DestinationRepathDistance)
        {
            return;
        }

        MoveTo(activeDestination, moveState);
    }
}
