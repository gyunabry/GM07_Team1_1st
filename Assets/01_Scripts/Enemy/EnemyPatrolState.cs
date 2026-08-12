using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolState : IState
{
    private Enemy enemy;
    private MonoBehaviour mono;
    private EnemyStateController stateController;
    private bool randomEnd = false;
    Vector3 moveDir;
    private Coroutine co;

    public EnemyPatrolState(Enemy enemy)
    {
        this.enemy = enemy;
        mono = enemy.GetComponent<MonoBehaviour>();
    }

    public void Enter()
    {
        enemy.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        moveDir = enemy.transform.forward;
        stateController = enemy.stateController;
        co = mono.StartCoroutine(RandomTime());
    }

    public void Execute()
    {
        float pDistance = enemy.agent.radius + 0.5f;
        Vector3 nextPos = enemy.agent.nextPosition;
        Vector3 probePos = nextPos + moveDir * pDistance;

        if(NavMesh.Raycast(nextPos, probePos, out NavMeshHit hit, NavMesh.AllAreas))
        {
            moveDir = Vector3.Reflect(moveDir, hit.normal);
            moveDir.y = 0f;
            moveDir.Normalize();
        }
        enemy.agent.Move(moveDir * Time.deltaTime * enemy.enemyData.PatrolSpeed);
        
        if (randomEnd == true)
        {
            stateController.ChangeState(stateController.IdleState);
        }
        if (enemy.isHit)
        {
            stateController.ChangeState(stateController.HitRunState);
            enemy.isHit = false;
        }
        if (enemy.CurrentHp <= 0)
        {
            stateController.ChangeState(stateController.DieState);
        }
    }

    public void Exit()
    {
        enemy.agent.ResetPath();
        mono.StopCoroutine(co);
        randomEnd = false;
    }
    public IEnumerator RandomTime()
    {
        yield return new WaitForSeconds(Random.Range(3f, 5f));
        randomEnd = true;
    }
}
