using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PlacedBuilding))]
public sealed class HunterBuildingController : MonoBehaviour
{
    private const float SpawnPointSampleDistance = 1f;
    [SerializeField] private HunterWorker hunterPrefab;
    [SerializeField] private Transform homePoint;

    private Transmitter transmitter;
    private readonly Dictionary<int,HunterWorker> workers=new(); 
    private EmployeeManager manager; 
    private PlacedBuilding building;
    private HuntingFieldContext areaContext;
    private Transform fallbackHomePoint;

    public bool TryGetEmployeeStats(out float attackDamage, out float movementSpeed, out int carryingCapacity)
    {
        attackDamage = 0f;
        movementSpeed = 0f;
        carryingCapacity = 0;

        if (hunterPrefab == null)
        {
            return false;
        }

        attackDamage = hunterPrefab.AttackDamage;
        movementSpeed = hunterPrefab.MovementSpeed;
        carryingCapacity = hunterPrefab.CarryingCapacity;
        return true;
    }

    private void Awake() 
    {
        building = GetComponent<PlacedBuilding>(); 
        if(homePoint==null) homePoint = transform;
    }

    private void OnEnable()
    {
        if (building != null)
        {
            building.OnConstructionCompleted += OnBuilt;
        }
    }

    private void Start()
    {
        manager = FindFirstObjectByType<EmployeeManager>();

        if (manager == null) return;

        manager.EmployeeHired += Hire; 
        manager.EmployeeRemoved += Remove;
        manager.HunterSkillModifiersChanged += ApplyHunterSkillModifiers;
        manager.HunterCarryingCapacityBonusChanged += ApplyHunterCarryingCapacityBonus;
        manager.AllEmployeeProcessingSpeedIncreaseChanged += ApplyAllEmployeeProcessingSpeedIncrease;
        manager.AllEmployeeMovementSpeedIncreaseChanged += ApplyAllEmployeeMovementSpeedIncrease; 

        if (building.IsComplete) Register();
    }

    private void OnDisable()
    {
        if (building != null) building.OnConstructionCompleted -= OnBuilt; 

        if (manager != null) 
        {
            manager.EmployeeHired-=Hire;
            manager.EmployeeRemoved-=Remove;
            manager.HunterSkillModifiersChanged -= ApplyHunterSkillModifiers;
            manager.HunterCarryingCapacityBonusChanged -= ApplyHunterCarryingCapacityBonus;
            manager.AllEmployeeProcessingSpeedIncreaseChanged -= ApplyAllEmployeeProcessingSpeedIncrease;
            manager.AllEmployeeMovementSpeedIncreaseChanged -= ApplyAllEmployeeMovementSpeedIncrease;
        }
    }

    private void OnDestroy()
    {
        if (manager!=null) manager.TryUnregisterBuilding(building);
        ReturnAllWorkers();
    }

    private void OnBuilt(PlacedBuilding b)
    {
        Register();
    } 

    private void Register()
    {
        if (manager == null || building == null)
        {
            return;
        }

        if (!TryResolveHuntingArea())
        {
            return;
        }

        manager.TryRegisterBuilding(building); 

        if (manager.TryGetEmployees(building,out var es))
        {
            foreach (var e in es)
            {
                Hire(e);
            }
        }   
    }

    private void Hire(EmployeeRuntimeData e)
    {
        if (e == null || e.Role != EmployeeRole.Hunter || e.AssignedBuilding != building || workers.ContainsKey(e.EmployeeId))
        {
            return;
        }

        PoolManager poolManager = PoolManager.Instance;

        if (poolManager == null) return;

        HunterWorker worker = poolManager.GetPool(hunterPrefab);

        if (worker == null) return;

        Transform workerHomePoint = ResolveWorkerHomePoint(worker);

        if (workerHomePoint == null || !worker.TryPlaceAt(workerHomePoint))
        {
            Debug.LogWarning($"HomePoint에 배치 실패했습니다.");

            worker.ResetForPool();
            poolManager.ReturnPool(worker);
            return;
        }

        worker.Initialize(
            manager, 
            e, 
            areaContext,
            transmitter, 
            workerHomePoint
        );
        ApplyHunterSkillModifiers(worker);
        worker.SetAllEmployeeProcessingSpeedIncreasePercent(manager.AllEmployeeProcessingSpeedIncreasePercent);
        worker.SetAllEmployeeMovementSpeedIncreasePercent(manager.AllEmployeeMovementSpeedIncreasePercent);
        worker.SetSkillCarryingCapacityBonus(manager.HunterCarryingCapacityBonus);

        workers[e.EmployeeId] = worker;
    }

    private void Remove(EmployeeRuntimeData e)
    {
        if (e!=null&&workers.TryGetValue(e.EmployeeId,out var w))
        {
            workers.Remove(e.EmployeeId);
            w.DepositCargoForBuildingSale();
            w.ResetForPool(); 
            PoolManager.Instance.ReturnPool(w);
        }
    }

    private void ReturnAllWorkers()
    {
        foreach(HunterWorker worker in workers.Values)
        {
            if (worker == null)continue;
            worker.DepositCargoForBuildingSale();
            worker.ResetForPool();
            if (PoolManager.Instance!=null) PoolManager.Instance.ReturnPool(worker);
        }

        workers.Clear();
    }

    private void ApplyHunterSkillModifiers(float damageIncreasePercent, float intervalReductionPercent, float rangeIncreasePercent)
    {
        foreach (HunterWorker worker in workers.Values)
        {
            worker?.SetSkillStatPercentModifiers(damageIncreasePercent, intervalReductionPercent, rangeIncreasePercent);
        }
    }

    private void ApplyHunterCarryingCapacityBonus(int amount)
    {
        foreach (HunterWorker worker in workers.Values)
        {
            worker?.SetSkillCarryingCapacityBonus(amount);
        }
    }

    private void ApplyAllEmployeeProcessingSpeedIncrease(float percent)
    {
        foreach (HunterWorker worker in workers.Values)
        {
            worker?.SetAllEmployeeProcessingSpeedIncreasePercent(percent);
        }
    }

    private void ApplyAllEmployeeMovementSpeedIncrease(float percent)
    {
        foreach (HunterWorker worker in workers.Values)
        {
            worker?.SetAllEmployeeMovementSpeedIncreasePercent(percent);
        }
    }

    private void ApplyHunterSkillModifiers(HunterWorker worker)
    {
        if (worker == null || manager == null)
        {
            return;
        }

        worker.SetSkillStatPercentModifiers(
            manager.HunterAttackDamageIncreasePercent,
            manager.HunterAttackIntervalReductionPercent,
            manager.HunterAttackRangeIncreasePercent);
    }

    private Transform ResolveWorkerHomePoint(HunterWorker worker)
    {
        if (worker == null || homePoint == null)
        {
            return null;
        }

        int areaMask = worker.AgentAreaMask;

        if (IsReachableSpawnPoint(homePoint.position, areaMask, out _))
        {
            return homePoint;
        }

        Vector3 preferredOffset = homePoint.position - transform.position;
        preferredOffset.y = 0f;

        float distance = preferredOffset.magnitude;
        Vector3 forward = distance > 0.01f ? preferredOffset / distance : transform.forward;
        forward.y = 0f;
        forward.Normalize();

        if (forward.sqrMagnitude < 0.01f)
        {
            forward = Vector3.forward;
        }

        distance = Mathf.Max(1f, distance);

        Vector3 right = new Vector3(forward.z, 0f, -forward.x);
        Vector3[] candidateDirections = { forward, right, -forward, -right };

        foreach (Vector3 direction in candidateDirections)
        {
            Vector3 candidate = transform.position + direction * distance;
            candidate.y = homePoint.position.y;

            if (!IsReachableSpawnPoint(candidate, areaMask, out NavMeshHit hit))
            {
                continue;
            }

            if (fallbackHomePoint == null)
            {
                fallbackHomePoint = new GameObject("HunterFallbackHomePoint").transform;
                fallbackHomePoint.SetParent(transform, true);
            }

            fallbackHomePoint.SetPositionAndRotation(hit.position, Quaternion.LookRotation(direction));
            return fallbackHomePoint;
        }

        return null;
    }

    private bool IsReachableSpawnPoint(Vector3 candidate, int areaMask, out NavMeshHit spawnHit)
    {
        return NavMesh.SamplePosition(candidate, out spawnHit, SpawnPointSampleDistance, areaMask);
    }
    private bool TryResolveHuntingArea()
    {
        BuildableArea assignedArea = building.AssignedArea;

        if (assignedArea == null)
        {
            return false;
        }

        areaContext = assignedArea.GetComponent<HuntingFieldContext>();

        if (areaContext == null || !areaContext.IsValid)
        {
            return false;
        }

        if (!areaContext.TryGetCompletedTransmitter(out transmitter))
        {
            return false;
        }

        return true;
    }

    public void SetTransmitter(Transmitter newTransmitter)
    {
        transmitter = newTransmitter;

        Register();
    }
}
