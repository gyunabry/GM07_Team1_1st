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
        Dropitem dropItem = poolManager.GetPool(dropItemPrefab);
        dropItem.Initialize(drop.Item, drop.Amount); // 해당 몬스터의 드랍 보상 정보를 주입
        dropItem.transform.position = transform.position;

        //foreach (var item in enemySO.dropItem)
        //{
        //    float chance = Random.Range(0f, 100f);
        //    if(item.dropChance >= chance)
        //    {
        //        Dropitem di = poolManager.GetPool<Dropitem>();
        //        di.GetDropItemData(item);
        //        di.transform.position = this.transform.position;
        //    }
        //}
    }

    // 몬스터 처치 시 해당 몬스터의 경험치만큼 플레이어에게 추가
    private void GrantExp()
    {
        int exp = enemyData.Reward.KillExp;
    }

    public void TakeDamage(float damage)
    {
        Debug.Log($"{name}: Player에게 {damage} 입음! 현재 체력 : {CurrentHp}");
        CurrentHp = CurrentHp - damage;
        isHit = true;
    }

    public void PlayDeathAnimation()
    {
        animationController.PlayDie();
    }
}