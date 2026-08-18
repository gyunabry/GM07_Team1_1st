using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawn : MonoBehaviour
{
    [SerializeField] private int maxEnemy;
    [SerializeField] private float enemySpawnTimer;
    [SerializeField] private Enemy enemy;
    [SerializeField] private MonsterPoolManager poolManager;
    [SerializeField] private BoxCollider spawnArea;
    [SerializeField] private EnemyDataSO enemyData;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastHeight;
    private Coroutine co;
    public List<Enemy> activeEnemy = new List<Enemy>();

    private void Update()
    {
        if(activeEnemy.Count < maxEnemy && co == null)
        {
            co = StartCoroutine(SpawnEnemy());
        }
    }
    private IEnumerator SpawnEnemy()
    {
        yield return new WaitForSeconds(enemySpawnTimer);
        while(activeEnemy.Count < maxEnemy)
        {
            Enemy nowEnemy = poolManager.GetPool<Enemy>();
            NavMeshAgent agent = nowEnemy.GetComponent<NavMeshAgent>();
            agent.Warp(RandomPoint());
            nowEnemy.GetEnemyData(enemyData);
            nowEnemy.enemySpawn = this;
            nowEnemy.CurrentHp = enemyData.Hp;
            nowEnemy.runStartDistance = enemyData.RunStartDistance;
            nowEnemy.runEndDistance = enemyData.RunEndDistance;
            activeEnemy.Add(nowEnemy);
        }
        co = null;
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
