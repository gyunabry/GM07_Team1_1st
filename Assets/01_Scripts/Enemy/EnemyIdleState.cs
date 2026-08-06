using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyIdleState : IState
{
    private Enemy enemy;
    private MonoBehaviour mono;
    private EnemyStateController stateController;
    private bool randomEnd = false;

    public EnemyIdleState(Enemy enemy)
    {
        this.enemy = enemy;
        mono = enemy.GetComponent<MonoBehaviour>();
    }
    public void Enter()
    {
        stateController = enemy.stateController;
        mono.StartCoroutine(RandomTime());
    }
    public void Execute()
    {
        if(randomEnd == true)
        {
            stateController.ChangeState(stateController.PatrolState);
        }
        Collider[] player = Physics.OverlapSphere(enemy.transform.position, enemy.runStartDistance, enemy.playerLayer);
        if (player.Length > 0)
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
    public IEnumerator RandomTime()
    {
        yield return new WaitForSeconds(Random.Range(1f, 3f));
        randomEnd = true;
    }
    public void Exit() 
    { 
        mono.StopCoroutine(RandomTime());
        randomEnd = false;
    }
}
