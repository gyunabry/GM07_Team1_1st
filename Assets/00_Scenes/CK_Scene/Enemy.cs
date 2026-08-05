using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

public class Enemy : MonoBehaviour
{
    public EnemyStateController stateController;
    public EnemySO enemySO;
    public LayerMask playerLayer;
    public NavMeshAgent agent;
    public float runDistance;
    public EnemySpawn enemySpawn;
    private MonsterPoolManager poolManager;
    private bool isStart = false;
    public int nowHp { get; set; }
   
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
    public void GetEnemyData(EnemySO enemySO)
    {
        this.enemySO = enemySO;
    }
    public void MonsterDie()
    {
        poolManager.ReturnPool(this);
    }
    public void MonsterDropItem()
    {
        foreach (var item in enemySO.dropItem)
        {
            float chance = Random.Range(0f, 100f);
            if(item.dropChance >= chance)
            {
                Dropitem di = poolManager.GetPool<Dropitem>();
                di.GetDropItemData(item);
                di.transform.position = this.transform.position;
            }
        }
    }
    public void TakeDamage(int damage)
    {
        nowHp = nowHp - damage;
    }
}
