using System.Collections;
using UnityEngine;

public class EnemyDieState : IState
{
    private Enemy enemy;
    private EnemyStateController stateController;

    public EnemyDieState(Enemy enemy)
    {
        this.enemy = enemy;
    }
    public void Enter()
    {
        stateController = enemy.stateController;
        enemy.enemySpawn.activeEnemy.Remove(enemy);
        enemy.MonsterDropItem();
        enemy.MonsterDie();
    }
    public void Execute()
    {

    }
    public void Exit()
    {

    }
    public IEnumerator RandomTime()
    {
        yield return null;
    }
}
