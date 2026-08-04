using System.Collections;
using UnityEngine;

public class EnemyRunState : IState
{
    private Enemy enemy;
    private EnemyStateController stateController;

    public EnemyRunState(Enemy enemy)
    {
        this.enemy = enemy;
    }
    public void Enter()
    {
        stateController = enemy.stateController;
    }
    public void Execute()
    {
        Collider[] player = Physics.OverlapSphere(enemy.transform.position, enemy.runDistance, enemy.playerLayer);
        Collider[] playerAttack = Physics.OverlapSphere(enemy.transform.position, 2f, enemy.playerLayer);
        if(player.Length > 0)
        {
            Vector3 dirPlayer = enemy.transform.position - player[0].transform.position;
            Vector3 runDistance = enemy.transform.position + dirPlayer.normalized * 1f;
            enemy.agent.SetDestination(runDistance);
        }
        else if(player.Length == 0)
        {
            stateController.ChangeState(stateController.IdleState);
        }
        if(playerAttack.Length > 0)
        {
            enemy.nowHp -= 1;
        }
        if (enemy.nowHp <= 0)
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
