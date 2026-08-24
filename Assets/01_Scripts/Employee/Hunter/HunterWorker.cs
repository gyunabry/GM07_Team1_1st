using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public sealed class HunterWorker : MonoBehaviour
{
    private const float NavMeshSampleDistance = 1f;

    private enum State { Idle, Trace, Attack, Get, Store }
    private static readonly Dictionary<Enemy, HunterWorker> MonsterOwners = new();
    private static readonly Dictionary<Dropitem, HunterWorker> DropOwners = new();
    private static readonly Dictionary<HunterWorker, Vector3> KillerDropReservations = new();

    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private float attackDamage = 5f;
    [SerializeField] private int carryingCapacity = 20;
    [SerializeField] private GameObject attackEffect;
    [SerializeField, Min(0.05f)] private float targetSearchInterval = 0.25f;

    private readonly HunterCargo cargo = new();
    private NavMeshAgent agent; 
    private EmployeeManager manager; 
    private EmployeeRuntimeData employee;
    private Collider area; 
    private Transmitter transmitter;
    private Transform home;
    private Enemy monster; 
    private Dropitem drop; 
    private State state; 
    private float nextAttack;
    private bool awaitingKillerDrop;
    private float killerDropWaitUntil;
    private HunterStatModifiers statModifiers;
    private float baseMovementSpeed;
    private float baseAttackRange;
    private float baseAttackDamage;
    private int baseCarryingCapacity;
    private NavMeshPath reusablePath;
    private float nextTargetSearchTime;

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
        baseAttackDamage = attackDamage;
        baseCarryingCapacity = carryingCapacity;
        reusablePath = new NavMeshPath();
    }
    private void Update()
    {
        if (employee == null) return;
        cargo.SetCapacity(carryingCapacity); // 스킬 서비스 연결 전 기본 한도 유지
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

    public void Initialize(
        EmployeeManager m, 
        EmployeeRuntimeData e, 
        Collider huntingArea,
        Transmitter targetTransmitter,
        Transform homePoint
    )
    {
        manager = m;
        employee = e;
        area = huntingArea;
        transmitter = targetTransmitter;
        home = homePoint;
        state = State.Idle; 
        cargo.Clear();
        awaitingKillerDrop = false;
        nextTargetSearchTime = 0f;

        ApplyStatModifiers(); 
        agent.stoppingDistance = 0.2f; 
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
        area = null;
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
        if (!Valid(monster)) { ReleaseMonster(); RequestTargetSearch(); state=State.Idle; return; }
        if (Distance(monster.transform.position) <= attackRange) { state=State.Attack; return; }
        if (!Move(monster.transform.position, EmployeeWorkState.Moving))
        {
            ReleaseMonster();
            RequestTargetSearch();
            state = State.Idle;
        }
    }
    private void Attack()
    {
        if (!Valid(monster)) { ReleaseMonster(); RequestTargetSearch(); state=State.Idle; return; }
        if (Distance(monster.transform.position) > attackRange) { state=State.Trace; return; }
        Stop(EmployeeWorkState.Working);
        if (Time.time < nextAttack) return;
        if (monster.CurrentHp <= attackDamage)
        {
            awaitingKillerDrop = true;
            killerDropWaitUntil = Time.time + 1f;
            KillerDropReservations[this] = monster.transform.position;
        }
        monster.TakeDamage(attackDamage);
        monster.stateController.ChangeState(new HunterFleeState(monster, transform));
        if (attackEffect != null) Instantiate(attackEffect, monster.transform.position, Quaternion.identity);
        nextAttack=Time.time+attackInterval;
    }

    private void Get()
    {
        if (!Valid(drop)) { ReleaseDrop(); RequestTargetSearch(); state=State.Idle; return; }
        if (cargo.Remaining <= 0) { state=State.Store; return; }
        if (Distance(drop.transform.position) > .3f)
        {
            if (!Move(drop.transform.position, EmployeeWorkState.Moving))
            {
                ReleaseDrop();
                RequestTargetSearch();
                state = State.Idle;
            }
            return;
        }

        Stop(EmployeeWorkState.Working);
        // cargo.Add(drop.Item, drop.TryCollectAmount(cargo.Remaining));
        cargo.Add(drop.Item, 1);

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

        if (Distance(transmitter.DepositPoint.position) > .3f)
        {
            Move(transmitter.DepositPoint.position, EmployeeWorkState.Moving);
            return;
        }

        int cargoBeforeDeposit = cargo.TotalAmount;
        cargo.TransferTo(transmitter.Inventory);

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
    private bool Valid(Enemy e)=>e!=null&&e.CurrentHp>0&&area!=null&&area.bounds.Contains(e.transform.position);
    private bool Valid(Dropitem d)=>d!=null&&d.Item!=null&&d.Amount>0&&area!=null&&area.bounds.Contains(d.transform.position);
    private bool TryClaimKillerDrop()
    {
        if (!KillerDropReservations.TryGetValue(this, out Vector3 deathPosition)) return false;
        foreach (Dropitem candidate in FindObjectsByType<Dropitem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (!Valid(candidate) || DropOwners.ContainsKey(candidate) || !IsAtSamePosition(candidate.transform.position, deathPosition)) continue;
            drop = candidate;
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
    // NavMesh 이동과 상호작용 범위는 지면(XZ) 기준이다. 드롭 프리팹과
    // 직원의 피벗 높이가 달라도, 수평으로 도착하면 획득할 수 있어야 한다.
    private float Distance(Vector3 p)
    {
        Vector3 offset = p - transform.position;
        offset.y = 0f;
        return offset.magnitude;
    }
    private void ApplyStatModifiers()
    {
        movementSpeed = Mathf.Max(0.1f, baseMovementSpeed + statModifiers.MovementSpeedBonus);
        carryingCapacity = Mathf.Max(1, baseCarryingCapacity + statModifiers.CarryingCapacityBonus);
        attackDamage = Mathf.Max(0f, baseAttackDamage + statModifiers.AttackDamageBonus);
        attackRange = Mathf.Max(0.1f, baseAttackRange + statModifiers.AttackRangeBonus);

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
    private void ReleaseMonster(){if(monster!=null&&MonsterOwners.TryGetValue(monster,out var o)&&o==this)MonsterOwners.Remove(monster);monster=null;}
    private void ReleaseDrop(){if(drop!=null&&DropOwners.TryGetValue(drop,out var o)&&o==this)DropOwners.Remove(drop);drop=null;}

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

        // 이전 경로를 제거
        if (agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }

        // agent 내부 위치를 해당 위치로 이동
        if (!agent.Warp(hit.position)) return false;
        transform.rotation = targetPoint.rotation;

        return true;
    }
}
