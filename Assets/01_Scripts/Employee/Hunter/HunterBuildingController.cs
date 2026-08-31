using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlacedBuilding))]
public sealed class HunterBuildingController : MonoBehaviour
{
    [SerializeField] private HunterWorker hunterPrefab;
    [SerializeField] private Transform homePoint;

    private Transmitter transmitter;
    private readonly Dictionary<int,HunterWorker> workers=new(); 
    private EmployeeManager manager; 
    private PlacedBuilding building;
    private HuntingFieldContext areaContext;

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

        if (!worker.TryPlaceAt(homePoint))
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
            homePoint
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
}
