using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyIdleState : IState
{
    private Enemy enemy;
    private MonoBehaviour mono;
    private EnemyStateController stateController;
    private bool randomEnd = false;
    private Coroutine co;

    public EnemyIdleState(Enemy enemy)
    {
        this.enemy = enemy;
        mono = enemy.GetComponent<MonoBehaviour>();
    }
    public void Enter()
    {
        stateController = enemy.stateController;
        co = mono.StartCoroutine(RandomTime());
    }
    public void Execute()
    {
        if(randomEnd == true)
        {
            stateController.ChangeState(stateController.PatrolState);
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
        mono.StopCoroutine(co);
        randomEnd = false;
    }
    public IEnumerator RandomTime()
    {
        yield return new WaitForSeconds(Random.Range(1f, 3f));
        randomEnd = true;
    }
    
}
