using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [SerializeField] private int maxEnemy;
    [SerializeField] private float enemySpawnTimer;
    [SerializeField] private Enemy enemy;
    [SerializeField] private MonsterPoolManager poolManager;
    [SerializeField] private Collider spawnArea;
    [SerializeField] private EnemySO enemySO;
    private Coroutine co;
    public List<Enemy> activeEnemy = new List<Enemy>();

    private void Update()
    {
        if(activeEnemy.Count < maxEnemy)
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
            nowEnemy.transform.position = RandomPoint();
            nowEnemy.GetEnemyData(enemySO);
            nowEnemy.enemySpawn = this;
            nowEnemy.nowHp = enemySO.hp;
            activeEnemy.Add(nowEnemy);
        }
        co = null;
    }
    private Vector3 RandomPoint()
    {
        Bounds bounds = spawnArea.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float z = Random.Range(bounds.min.z, bounds.max.z);

        return new Vector3(x, bounds.min.y, z);
    }
}
