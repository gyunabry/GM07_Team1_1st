using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public NavMeshAgent agent;
    private PoolManager poolManager;

    public EnemyStateController stateController;
    [SerializeField] private EnemyAnimationController animationController;
    public EnemyDataSO enemyData;
    public Dropitem dropItemPrefab;
    public LayerMask playerLayer;

    public float runStartDistance;
    public float runEndDistance;
    public EnemySpawn enemySpawn;

    private bool isStart = false;
    private bool isDying;

    public bool isHit = false;

    public float CurrentHp { get; set; }

    public EnemyAnimationController AnimationController => animationController;

    private void OnEnable()
    {
        poolManager = FindAnyObjectByType<PoolManager>();
        if (!isStart)
        {
            isStart = true;
            return;
        }
        agent = GetComponent<NavMeshAgent>();
        stateController = new EnemyStateController(this);
        stateController.ChangeState(stateController.IdleState);

        isDying = false;
        isHit = false;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
    }

    private void Update()
    {
        stateController.UpdateExcute();

        if (agent != null && animationController != null)
        {
            animationController.SetMoveSpeed(agent.velocity.magnitude);
        }
    }

    public void GetEnemyData(EnemyDataSO enemySO)
    {
        this.enemyData = enemySO;
    }

    public void MonsterDie()
    {
        poolManager.ReturnPool(this);
    }

    public void MonsterDropItem()
    {
        ItemAmount drop = enemyData.Reward.Drop;

        // 기본 드랍
        SpawnDropItem(drop);
    
        // 현재 아이템 추가 드랍 확률에 따라 확률적으로 추가 드랍
        if (RewardSkillRegistry.RollAdditionalItemDrop())
        {
            SpawnDropItem(drop);
        }
    }

    // 드랍 아이템을 스폰하는 메서드
    private void SpawnDropItem(ItemAmount drop)
    {
        if (!drop.IsValid || poolManager == null || dropItemPrefab == null)
        {
            return;
        }

        // 프리팹 생성
        Dropitem spawnedDrop = poolManager.GetPool(dropItemPrefab);
        if (spawnedDrop == null) return;

        // 해당 몬스터의 드랍 보상 정보를 주입
        spawnedDrop.Initialize(drop.Item, drop.Amount); 
        spawnedDrop.transform.position = transform.position;
    }

    // 몬스터 처치 시 해당 몬스터의 경험치만큼 플레이어에게 추가
    public void GrantExp()
    {
        int baseExp = enemyData.Reward.KillExp;
        int finalExp = RewardSkillRegistry.ApplyHuntExperience(baseExp);

        CurrencySystem.Instance.GrantExperience(finalExp);
    }

    public void TakeDamage(float damage)
    {
        CurrentHp = CurrentHp - damage;
        isHit = true;
    }

    public void PlayDeathAnimation()
    {
        animationController.PlayDie();
    }
}