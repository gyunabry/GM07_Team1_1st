using System.Collections;
using UnityEngine;

/// <summary>사냥 직원에게 피격된 몬스터를 공격자 반대편으로 도망가게 합니다.</summary>
public sealed class HunterFleeState : IState
{
    private readonly Enemy enemy;
    private readonly Transform attacker;
    private float endTime;
    public HunterFleeState(Enemy enemy, Transform attacker) { this.enemy = enemy; this.attacker = attacker; }
    public void Enter()
    {
        // 기존 Enemy 상태는 isHit를 플레이어 피격 신호로 사용한다.
        // 사냥 직원 피격은 이 전용 상태에서 처리하므로 신호를 소비한다.
        enemy.isHit = false;
        endTime = Time.time + 3f;
    }
    public void Execute()
    {
        if (enemy.CurrentHp <= 0f) { enemy.stateController.ChangeState(enemy.stateController.DieState); return; }
        if (attacker == null || Time.time >= endTime) { enemy.stateController.ChangeState(enemy.stateController.IdleState); return; }
        Vector3 direction = (enemy.transform.position - attacker.position).normalized;
        enemy.agent.SetDestination(enemy.transform.position + direction * 2f);
    }
    public void Exit() { if (enemy.agent != null && enemy.agent.isOnNavMesh) enemy.agent.ResetPath(); }
    public IEnumerator RandomTime() { yield return null; }
}
