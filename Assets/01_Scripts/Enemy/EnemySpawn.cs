using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawn : MonoBehaviour
{
    [Header("몬스터 설정")]
    [SerializeField] private EnemySpawnEntry enemyEntry;
    [SerializeField] private float enemySpawnTimer = 1f;

    [Header("스폰 구역")]
    [SerializeField] private BoxCollider spawnArea;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastHeight = 10f;

    [Header("풀")]
    [SerializeField] private MonsterPoolManager poolManager;

    private Coroutine spawnCoroutine;
    private WaitForSeconds spawnWait;

    public List<Enemy> activeEnemy = new List<Enemy>();

    private void Awake()
    {
        spawnWait = new WaitForSeconds(enemySpawnTimer);
    }

    private void Start()
    {
        if (poolManager == null)
        {
            poolManager = MonsterPoolManager.Instance;
        }
    }

    private void Update()
    {
        if(activeEnemy.Count >= enemyEntry.maxEnemyCount)
        {
            return;
        }

        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnEnemy());
        }
    }

    private IEnumerator SpawnEnemy()
    {
        yield return spawnWait;

        if (activeEnemy.Count < enemyEntry.maxEnemyCount)
        {
            SpawnOneEnemy();
        }

        spawnCoroutine = null;
    }

    private void SpawnOneEnemy()
    {
        Enemy enemy = poolManager.GetPool(enemyEntry.prefab);

        if (enemy == null) return;

        EnemyDataSO data = enemyEntry.data;

        enemy.GetEnemyData(data);
        enemy.enemySpawn = this;
        enemy.CurrentHp = data.Hp;
        enemy.runStartDistance = data.RunStartDistance;
        enemy.runEndDistance = data.RunEndDistance;

        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        agent.Warp(RandomPoint());

        activeEnemy.Add(enemy);
    }

    private Vector3 RandomPoint()
    {
        Vector3 localPoint = spawnArea.center + new Vector3(Random.Range(-spawnArea.size.x * 0.5f, spawnArea.size.x * 0.5f),
            0f,
            Random.Range(-spawnArea.size.z * 0.5f, spawnArea.size.z * 0.5f));

        Vector3 rayOri = spawnArea.transform.TransformPoint(localPoint);
        rayOri += Vector3.up * raycastHeight;
        if(Physics.Raycast(rayOri, Vector3.down, out RaycastHit hit, raycastHeight * 2, groundLayer))
        {
            return hit.point;
        }
        return spawnArea.transform.TransformPoint(localPoint);
    }
}
