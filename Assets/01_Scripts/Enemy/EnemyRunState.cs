using System.Collections;
using UnityEngine;

public class EnemyRunState : IState
{
    private Enemy enemy;
    private EnemyStateController stateController;
    private Player player;

    public EnemyRunState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        stateController = enemy.stateController;
        Collider[] cPlayer = Physics.OverlapSphere(enemy.transform.position, enemy.runEndDistance, enemy.playerLayer);
        player = cPlayer[0].GetComponent<Player>();
    }

    public void Execute()
    {
            Vector3 dirPlayer = enemy.transform.position - player.transform.position;
        if(Vector3.Distance(player.transform.position, enemy.transform.position) <= enemy.runEndDistance)
        {
            Vector3 runDistance = enemy.transform.position + dirPlayer.normalized * 1f;
            enemy.agent.SetDestination(runDistance);
        }
        else
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
    }

    public IEnumerator RandomTime()
    {
        yield return null;
    }
}
