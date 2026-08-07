using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public NavMeshAgent agent;
    private MonsterPoolManager poolManager;

    public EnemyStateController stateController;
    public EnemyDataSO enemyData;
    public LayerMask playerLayer;

    public float runStartDistance;
    public float runEndDistance;
    public EnemySpawn enemySpawn;
    
    private bool isStart = false;
    public bool isHit = false;

    public int CurrentHp { get; set; }
   
    private void OnEnable()
    {
        poolManager = FindAnyObjectByType<MonsterPoolManager>();
        if (!isStart)
        {
            isStart = true;
            return;
        }
        agent = GetComponent<NavMeshAgent>();
        stateController = new EnemyStateController(this);
        stateController.ChangeState(stateController.IdleState);
    }

    private void Update()
    {
        stateController.UpdateExcute();
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

    public void TakeDamage(int damage)
    {
        CurrentHp = CurrentHp - damage;
        isHit = true;
    }
}
