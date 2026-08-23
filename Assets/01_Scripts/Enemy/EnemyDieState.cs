using System.Collections;
using UnityEngine;

public class EnemyDieState : IState
{
    private Enemy enemy;
    private EnemyStateController stateController;

    private bool deathCompleted;

    public EnemyDieState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        deathCompleted = false;

        enemy.enemySpawn.activeEnemy.Remove(enemy);

        if (enemy.agent != null && enemy.agent.isOnNavMesh)
        {
            enemy.agent.ResetPath();
            enemy.agent.isStopped = true;
        }

        enemy.PlayDeathAnimation();

        // stateController = enemy.stateController;
        
        //enemy.MonsterDropItem();
        //enemy.MonsterDie();
    }

    public void Execute()
    {

    }

    public void Exit()
    {

    }

    public void CompleteDeath()
    {
        if (deathCompleted) return;

        deathCompleted = true;

        enemy.MonsterDropItem();
        enemy.MonsterDie();
    }

    public IEnumerator RandomTime()
    {
        yield return null;
    }
}
