using System.Collections;
using UnityEngine;

public class EnemyHitRunState : IState
{
    private Enemy enemy;
    private EnemyStateController stateController;
    private MonoBehaviour mono;
    private bool isRunEnd = false;
    public EnemyHitRunState(Enemy enemy)
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
        Collider[] player = Physics.OverlapSphere(enemy.transform.position, enemy.runEndDistance * 30, enemy.playerLayer);

        if (player.Length > 0)
        {
            Vector3 dirPlayer = enemy.transform.position - player[0].transform.position;
            Vector3 runDistance = enemy.transform.position + dirPlayer.normalized * 1f;
            enemy.agent.SetDestination(runDistance);
        }
        else if (player.Length == 0)
        {
            stateController.ChangeState(stateController.IdleState);
        }
        if(isRunEnd == true)
        {
            stateController.ChangeState(stateController.IdleState);
        }

        if (enemy.CurrentHp <= 0)
        {
            stateController.ChangeState(stateController.DieState);
        }
    }
    public void Exit()
    {
        enemy.agent.ResetPath();
        mono.StopCoroutine(RandomTime());
        isRunEnd = false;
    }
    public IEnumerator RandomTime()
    {
        yield return new WaitForSeconds(3f);
        isRunEnd = true;
    }
}
