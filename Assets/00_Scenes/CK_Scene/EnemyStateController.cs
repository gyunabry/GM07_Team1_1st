using System.Collections;
using UnityEngine;

public class EnemyStateController
{
    public IState nowState;
    public EnemyIdleState IdleState { get; private set; }
    public EnemyPatrolState PatrolState { get; private set; }
    public EnemyRunState RunState { get; private set; }
    public EnemyDieState DieState { get; private set; }

    public EnemyStateController(Enemy enemy)
    {
        IdleState = new EnemyIdleState(enemy);
        PatrolState = new EnemyPatrolState(enemy);
        RunState = new EnemyRunState(enemy);
        DieState = new EnemyDieState(enemy);
    }
    public void ChangeState(IState newState)
    {
        if (newState == null) return;
        if (nowState == newState) return;
        if (nowState != null) nowState.Exit();
        nowState = newState;
        nowState.Enter();
        IEnumerator stateRoutine = nowState.RandomTime();
        
    }
    public void UpdateExcute()
    {
        if(nowState != null) nowState.Execute();
    }
}
public interface IState
{
    void Enter();
    void Execute();
    void Exit();
    IEnumerator RandomTime();
}