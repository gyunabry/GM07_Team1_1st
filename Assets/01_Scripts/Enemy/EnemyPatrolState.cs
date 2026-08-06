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
        mono.StartCoroutine(RandomTime());
    }
    public void Execute()
    {
        enemy.agent.Move(moveDir * Time.deltaTime * enemy.enemySO.patrolSpeed);
        
        if (randomEnd == true)
        {
            stateController.ChangeState(stateController.IdleState);
        }
        Collider[] player = Physics.OverlapSphere(enemy.transform.position, enemy.runStartDistance, enemy.playerLayer);
        if(player.Length > 0)
        {
            stateController.ChangeState(stateController.RunState);
        }
        if (enemy.isHit)
        {
            stateController.ChangeState(stateController.HitRunState);
            enemy.isHit = false;
        }
        if (enemy.nowHp <= 0)
        {
            stateController.ChangeState(stateController.DieState);
        }
    }
    public void Exit()
    {
        enemy.agent.ResetPath();
        mono.StopCoroutine(RandomTime());
        randomEnd = false;
    }
    public IEnumerator RandomTime()
    {
        yield return new WaitForSeconds(Random.Range(3f, 5f));
        randomEnd = true;
    }
}
