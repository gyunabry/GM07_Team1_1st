using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public sealed class HunterWorker : MonoBehaviour
{
    private const float NavMeshSampleDistance = 1f;
    private const float MonsterPathFailureGraceDuration = 1.5f;

    private enum State { Idle, Trace, Attack, Get, Store }
    private static readonly Dictionary<Enemy, HunterWorker> MonsterOwners = new();
    private static readonly Dictionary<Dropitem, HunterWorker> DropOwners = new();
    private static readonly Dictionary<HunterWorker, Vector3> KillerDropReservations = new();

    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private float attackDamage = 5f;
    [SerializeField, Min(0.05f)] private float itemPickupDuration = 10f;
    [SerializeField, Min(0.05f)] private float itemDeliveryDuration = 10f;
    [SerializeField] private int carryingCapacity = 20;
    [SerializeField] private GameObject attackEffect;
    [SerializeField, Min(0.05f)] private float targetSearchInterval = 0.25f;
    [SerializeField, TextArea(2, 4)] private string runtimeDebugStatus;

    private readonly HunterCargo cargo = new();
    private NavMeshAgent agent; 
    private EmployeeManager manager; 
    private EmployeeRuntimeData employee;
    private HuntingFieldContext huntingField;
    private Transmitter transmitter;
    private Transform home;
    private Enemy monster; 
    private Dropitem drop; 
    private State state; 
    private float nextAttack;
    private bool awaitingKillerDrop;
    private bool collectingKillerDrop;
    private float killerDropWaitUntil;
    private HunterStatModifiers statModifiers;
    private float baseMovementSpeed;
    private float baseAttackRange;
    private float baseAttackInterval;
    private float baseAttackDamage;
    private int baseCarryingCapacity;
    private float attackDamageIncreasePercent;
    private float attackIntervalReductionPercent;
    private float attackRangeIncreasePercent;
    private int skillCarryingCapacityBonus;
    private float itemPickupElapsed;
    private float itemDeliveryElapsed;
    private float allEmployeeProcessingSpeedIncreasePercent;
    private float allEmployeeMovementSpeedIncreasePercent;
    private NavMeshPath reusablePath;
    private float nextTargetSearchTime;
    private float monsterPathFailureSince = -1f;

    public float MovementSpeed => movementSpeed;
    public float AttackDamage => attackDamage;
    public int CarryingCapacity => carryingCapacity;
    public int AgentAreaMask => agent != null ? agent.areaMask : NavMesh.AllAreas;

    public string DebugStatus
    {
        get
        {
            float targetDistance = drop != null ? Distance(drop.transform.position) :
                monster != null ? Distance(monster.transform.position) : -1f;
            return $"state={state}, cargo={cargo.TotalAmount}/{cargo.Capacity}, " +
                $"monster={(monster != null ? monster.name : "none")}, " +
                $"drop={(drop != null ? drop.name : "none")}, " +
                $"targetDistance={targetDistance:F2}, remaining={(agent != null ? agent.remainingDistance : -1f):F2}, " +
                $"hasPath={(agent != null && agent.hasPath)}, stopped={(agent != null && agent.isStopped)}, " +
                $"onNavMesh={(agent != null && agent.isOnNavMesh)}";
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        baseMovementSpeed = movementSpeed;
        baseAttackRange = attackRange;
        baseAttackInterval = attackInterval;
        baseAttackDamage = attackDamage;
        baseCarryingCapacity = carryingCapacity;
        reusablePath = new NavMeshPath();
    }
    private void Update()
    {
        if (employee == null) return;
        cargo.SetCapacity(carryingCapacity); // ?§ÌÇ¨ ?úÎπÑ???∞Í≤∞ ??Í∏∞Î≥∏ ?úÎèÑ ?†Ï?
        if (awaitingKillerDrop)
        {
            if (TryClaimKillerDrop())
            {
                awaitingKillerDrop = false;
                KillerDropReservations.Remove(this);
                ReleaseMonster();
                state = State.Get;
            }
            else if (Time.time < killerDropWaitUntil)
            {
                Stop(EmployeeWorkState.Idle);
                return;
            }
            else
            {
                awaitingKillerDrop = false;
                KillerDropReservations.Remove(this);
                ReleaseMonster();
            }
        }

        if (cargo.TotalAmount >= cargo.Capacity) 
        { 
            ReleaseMonster(); 
            state = State.Store; 
        }

        switch (state) { 
            case State.Idle: 
                Decide(); 
                break; 

            case State.Trace: 
                Trace(); 
                break; 

            case State.Attack: 
                Attack(); 
                break; 

            case State.Get:
                Get(); 
                break;

            case State.Store: 
                Store();
                break; 
        }
    }

    private void LateUpdate()
    {
        runtimeDebugStatus = DebugStatus;
    }

    public void Initialize(
        EmployeeManager m, 
        EmployeeRuntimeData e, 
        HuntingFieldContext assignedHuntingField,
        Transmitter targetTransmitter,
        Transform homePoint
    )
    {
        manager = m;
        employee = e;
        huntingField = assignedHuntingField;
        transmitter = targetTransmitter;
        home = homePoint;
        state = State.Idle; 
        cargo.Clear();
        awaitingKillerDrop = false;
        collectingKillerDrop = false;
        nextTargetSearchTime = 0f;
        monsterPathFailureSince = -1f;
        itemPickupElapsed = 0f;
        itemDeliveryElapsed = 0f;

        ApplyStatModifiers(); 
        agent.stoppingDistance = 1.5f; 
        manager.TrySetWorkState(employee, EmployeeWorkState.Idle);
    }

    public void SetCarryingCapacity(int value)
    {
        baseCarryingCapacity = Mathf.Max(1, value);
        ApplyStatModifiers();
    }
    public void SetStatModifiers(HunterStatModifiers modifiers)
    {
        statModifiers = modifiers;
        ApplyStatModifiers();
    }

    public void SetSkillStatPercentModifiers(float damageIncreasePercent, float intervalReductionPercent, float rangeIncreasePercent)
    {
        attackDamageIncreasePercent = Mathf.Max(0f, damageIncreasePercent);
        attackIntervalReductionPercent = Mathf.Clamp(intervalReductionPercent, 0f, 100f);
        attackRangeIncreasePercent = Mathf.Max(0f, rangeIncreasePercent);
        ApplyStatModifiers();
    }

    public void SetSkillCarryingCapacityBonus(int amount)
    {
        skillCarryingCapacityBonus = Mathf.Max(0, amount);
        ApplyStatModifiers();
    }

    public void SetAllEmployeeProcessingSpeedIncreasePercent(float percent)
    {
        allEmployeeProcessingSpeedIncreasePercent = Mathf.Clamp(percent, 0f, 100f);
    }

    public void SetAllEmployeeMovementSpeedIncreasePercent(float percent)
    {
        allEmployeeMovementSpeedIncreasePercent = Mathf.Max(0f, percent);
        ApplyStatModifiers();
    }

    public HunterStatModifiers GetStatModifiers() => statModifiers;

    public void DepositCargoForBuildingSale()
    {
        cargo.TransferTo(transmitter?.Inventory);
        cargo.Clear();
    }

    public void ResetForPool() 
    { 
        ReleaseTargets(); 
        cargo.Clear(); 

        manager = null;
        employee = null;
        huntingField = null;
        transmitter = null;
        home=null;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }   
    }

    private void RequestTargetSearch() => nextTargetSearchTime = 0f;

    private void Decide()
    {
        if (cargo.TotalAmount >= cargo.Capacity) { state=State.Store; return; }
        if (Valid(drop)) { state = State.Get; return; }
        if (drop != null) ReleaseDrop();
        if (Time.time < nextTargetSearchTime) return;
        nextTargetSearchTime = Time.time + targetSearchInterval;
        if (ClaimDrop()) { state=State.Get; return; }
        if (ClaimMonster()) { state=State.Trace; return; }
        Move(home != null ? home.position : transform.position, EmployeeWorkState.Idle);
    }
    private void Trace()
    {
        if (!HasActiveMonsterTarget()) { ReleaseMonster(); RequestTargetSearch(); state = State.Idle; return; }
        if (Distance(monster.transform.position) <= attackRange)
        {
            monsterPathFailureSince = -1f;
            state = State.Attack;
            return;
        }

        if (Move(monster.transform.position, EmployeeWorkState.Moving))
        {
            monsterPathFailureSince = -1f;
            return;
        }

        if (monsterPathFailureSince < 0f)
        {
            monsterPathFailureSince = Time.time;
        }

        if (Time.time - monsterPathFailureSince < MonsterPathFailureGraceDuration)
        {
            return;
        }

        ReleaseMonster();
        RequestTargetSearch();
        state = State.Idle;
    }
    private void Attack()
    {
        if (!HasActiveMonsterTarget()) { ReleaseMonster(); RequestTargetSearch(); state = State.Idle; return; }
        if (Distance(monster.transform.position) > attackRange) { state=State.Trace; return; }
        Stop(EmployeeWorkState.Working);
        if (Time.time < nextAttack) return;
        if (monster.CurrentHp <= attackDamage)
        {
            awaitingKillerDrop = true;
            killerDropWaitUntil = Time.time + 3f;
            KillerDropReservations[this] = monster.transform.position;
        }
        monster.TakeDamage(attackDamage);
        monster.stateController.ChangeState(new HunterFleeState(monster, transform));
        if (attackEffect != null) Instantiate(attackEffect, monster.transform.position, Quaternion.identity);
        nextAttack=Time.time+attackInterval;
    }

    private void Get()
    {
        if (!IsValidDropTarget(drop)) { ReleaseDrop(); RequestTargetSearch(); state=State.Idle; return; }
        if (cargo.Remaining <= 0) { state=State.Store; return; }
        if (Distance(drop.transform.position) > 1.5f)
        {
            itemPickupElapsed = 0f;
            if (!Move(drop.transform.position, EmployeeWorkState.Moving))
            {
                ReleaseDrop();
                RequestTargetSearch();
                state = State.Idle;
            }
            return;
        }

        ItemDataSO item = drop.Item;
        int collectedAmount = drop.TryCollectAmount(cargo.Remaining);
        itemPickupElapsed = 0f;

        if (collectedAmount <= 0)
        {
            ReleaseDrop();
            RequestTargetSearch();
            state = State.Idle;
            return;
        }

        cargo.Add(item, collectedAmount);

        if (Valid(drop))
        {
            state = cargo.Remaining <= 0 ? State.Store : State.Get;
            return;
        }
        ReleaseDrop();
        RequestTargetSearch();
        state=State.Idle;
    }

    private void Store()
    {
        if (transmitter == null) 
        { 
            Stop(EmployeeWorkState.Idle);
            return; 
        }

        if (Distance(transmitter.DepositPoint.position) > 1.3f)
        {
            itemDeliveryElapsed = 0f;
            Move(transmitter.DepositPoint.position, EmployeeWorkState.Moving);
            return;
        }

        Stop(EmployeeWorkState.Working);
        if (!TryCompleteItemDelivery()) return;

        int cargoBeforeDeposit = cargo.TotalAmount;
        cargo.TransferTo(transmitter.Inventory);
        itemDeliveryElapsed = 0f;

        Stop(cargo.TotalAmount == cargoBeforeDeposit ? EmployeeWorkState.Idle : EmployeeWorkState.Working);

        if (cargo.TotalAmount < cargo.Capacity)
        { 
            RequestTargetSearch(); 
            state=State.Idle; 
        }
    }

    private bool ClaimMonster()
    {
        CleanupReservations();
        Enemy closest = null;
        float closestDistance = float.MaxValue;
        foreach (Enemy candidate in FindObjectsByType<Enemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (!Valid(candidate) || MonsterOwners.ContainsKey(candidate) || !CanReach(candidate.transform.position)) continue;
            float distance = Distance(candidate.transform.position);
            if (distance < closestDistance) { closest = candidate; closestDistance = distance; }
        }
        if (closest == null) return false;
        monster = closest;
        MonsterOwners[monster] = this;
        return true;
    }

    private bool ClaimDrop()
    {
        CleanupReservations();
        Dropitem closest = null;
        float closestDistance = float.MaxValue;
        foreach (Dropitem candidate in FindObjectsByType<Dropitem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (!Valid(candidate) || DropOwners.ContainsKey(candidate) || IsReservedForAnotherHunter(candidate) || !CanReach(candidate.transform.position)) continue;
            float distance = Distance(candidate.transform.position);
            if (distance < closestDistance) { closest = candidate; closestDistance = distance; }
        }
        if (closest == null) return false;
        drop = closest;
        DropOwners[drop] = this;
        return true;
    }
    private bool Valid(Enemy e) => e != null && e.CurrentHp > 0 && IsInsideHuntingArea(e.transform.position);
    private bool Valid(Dropitem d) => d != null && d.Item != null && d.Amount > 0 && IsInsideHuntingArea(d.transform.position);
    private bool IsValidDropTarget(Dropitem d) => d != null && d.Item != null && d.Amount > 0 &&
                                                     (collectingKillerDrop || IsInsideHuntingArea(d.transform.position));

    private bool IsInsideHuntingArea(Vector3 position)
    {
        return huntingField != null && huntingField.ContainsHuntingPosition(position);
    }
    private bool TryClaimKillerDrop()
    {
        if (!KillerDropReservations.TryGetValue(this, out Vector3 deathPosition)) return false;
        foreach (Dropitem candidate in FindObjectsByType<Dropitem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (candidate == null || candidate.Item == null || candidate.Amount <= 0 || DropOwners.ContainsKey(candidate) || !IsAtSamePosition(candidate.transform.position, deathPosition)) continue;
            drop = candidate;
            collectingKillerDrop = true;
            DropOwners[drop] = this;
            return true;
        }
        return false;
    }

    private bool IsReservedForAnotherHunter(Dropitem candidate)
    {
        foreach (KeyValuePair<HunterWorker, Vector3> reservation in KillerDropReservations)
        {
            if (reservation.Key != this && IsAtSamePosition(candidate.transform.position, reservation.Value)) return true;
        }
        return false;
    }

    private static bool IsAtSamePosition(Vector3 first, Vector3 second)
    {
        first.y = 0f;
        second.y = 0f;
        return (first - second).sqrMagnitude <= 0.25f;
    }
    // NavMesh ?¥ÎèôÍ≥??ÅÌò∏?ëÏö© Î≤îÏúÑ??ÏßÄÎ©?XZ) Í∏∞Ï??¥Îã§. ?úÎ°≠ ?ÑÎ¶¨?πÍ≥º
    // ÏßÅÏõê???ºÎ≤ó ?íÏù¥Í∞Ä ?¨Îùº?? ?òÌèâ?ºÎ°ú ?ÑÏ∞©?òÎ©¥ ?çÎìù?????àÏñ¥???úÎã§.
    private float Distance(Vector3 p)
    {
        Vector3 offset = p - transform.position;
        offset.y = 0f;
        return offset.magnitude;
    }
    private bool TryCompleteItemPickup()
    {
        itemPickupElapsed += Time.deltaTime;
        float duration = Mathf.Max(0.05f, itemPickupDuration * (1f - allEmployeeProcessingSpeedIncreasePercent / 100f));
        return itemPickupElapsed >= duration;
    }

    private bool TryCompleteItemDelivery()
    {
        itemDeliveryElapsed += Time.deltaTime;
        float duration = Mathf.Max(0.05f, itemDeliveryDuration * (1f - allEmployeeProcessingSpeedIncreasePercent / 100f));
        return itemDeliveryElapsed >= duration;
    }

    private void ApplyStatModifiers()
    {
        movementSpeed = Mathf.Max(0.1f, (baseMovementSpeed + statModifiers.MovementSpeedBonus) * (1f + allEmployeeMovementSpeedIncreasePercent / 100f));
        carryingCapacity = Mathf.Max(1, baseCarryingCapacity + statModifiers.CarryingCapacityBonus + skillCarryingCapacityBonus);
        attackDamage = Mathf.Max(0f, baseAttackDamage * (1f + attackDamageIncreasePercent / 100f) + statModifiers.AttackDamageBonus);
        attackInterval = Mathf.Max(0.05f, baseAttackInterval * (1f - attackIntervalReductionPercent / 100f));
        attackRange = Mathf.Max(0.1f, baseAttackRange * (1f + attackRangeIncreasePercent / 100f) + statModifiers.AttackRangeBonus);

        if (agent != null) agent.speed = movementSpeed;
        cargo.SetCapacity(carryingCapacity);
    }
    private bool Move(Vector3 p, EmployeeWorkState s)
    {
        if (!CanReach(p)) return false;
        NavMesh.SamplePosition(p, out NavMeshHit hit, 3f, agent.areaMask);
        agent.isStopped = false;
        agent.SetDestination(hit.position);
        manager.TrySetWorkState(employee, s);
        return true;
    }

    private bool CanReach(Vector3 target)
    {
        if (agent == null || !agent.isOnNavMesh || !NavMesh.SamplePosition(target, out NavMeshHit hit, 3f, agent.areaMask)) return false;
        reusablePath ??= new NavMeshPath();
        return agent.CalculatePath(hit.position, reusablePath) && reusablePath.status == NavMeshPathStatus.PathComplete;
    }

    private static void CleanupReservations()
    {
        List<Enemy> monstersToRelease = new();
        foreach (KeyValuePair<Enemy, HunterWorker> pair in MonsterOwners)
            if (pair.Key == null || !pair.Key.gameObject.activeInHierarchy || pair.Value == null) monstersToRelease.Add(pair.Key);
        foreach (Enemy target in monstersToRelease) MonsterOwners.Remove(target);

        List<Dropitem> dropsToRelease = new();
        foreach (KeyValuePair<Dropitem, HunterWorker> pair in DropOwners)
            if (pair.Key == null || !pair.Key.gameObject.activeInHierarchy || pair.Value == null) dropsToRelease.Add(pair.Key);
        foreach (Dropitem target in dropsToRelease) DropOwners.Remove(target);

        List<HunterWorker> workersToRelease = new();
        foreach (KeyValuePair<HunterWorker, Vector3> pair in KillerDropReservations)
            if (pair.Key == null || !pair.Key.gameObject.activeInHierarchy) workersToRelease.Add(pair.Key);
        foreach (HunterWorker worker in workersToRelease) KillerDropReservations.Remove(worker);
    }
    private void Stop(EmployeeWorkState s){if(agent.isOnNavMesh){agent.ResetPath();agent.isStopped=true;}manager.TrySetWorkState(employee,s);}
    private void ReleaseTargets(){awaitingKillerDrop=false;KillerDropReservations.Remove(this);ReleaseMonster();ReleaseDrop();}
    private bool HasActiveMonsterTarget() => monster != null && monster.CurrentHp > 0f;

    private void ReleaseMonster()
    {
        monsterPathFailureSince = -1f;
        if (monster != null && MonsterOwners.TryGetValue(monster, out var owner) && owner == this)
        {
            MonsterOwners.Remove(monster);
        }

        monster = null;
    }
    private void ReleaseDrop(){if(drop!=null&&DropOwners.TryGetValue(drop,out var o)&&o==this)DropOwners.Remove(drop);drop=null;collectingKillerDrop=false;}

    public bool TryPlaceAt(Transform targetPoint)
    {
        if (targetPoint == null || agent == null) return false;

        if (!agent.enabled) agent.enabled = true;

        if (!NavMesh.SamplePosition(
            targetPoint.position,
            out NavMeshHit hit,
            NavMeshSampleDistance,
            agent.areaMask))
        {
            return false;
        }

        // ?¥Ï†Ñ Í≤ΩÎ°úÎ•??úÍ±∞
        if (agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }

        // agent ?¥Î? ?ÑÏπòÎ•??¥Îãπ ?ÑÏπòÎ°??¥Îèô
        if (!agent.Warp(hit.position)) return false;
        transform.rotation = targetPoint.rotation;

        return true;
    }
}
