using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyHitRunState : IState
{
    private Enemy enemy;
    private EnemyStateController stateController;
    private MonoBehaviour mono;
    private Player player;
    private bool isRunEnd = false;

    public EnemyHitRunState(Enemy enemy)
    {
        this.enemy = enemy;
        mono = enemy.GetComponent<MonoBehaviour>();
    }

    public void Enter()
    {
        stateController = enemy.stateController;

        enemy.AnimationController.PlayHit();

        mono.StartCoroutine(RandomTime());
        Collider[] cPlayer = Physics.OverlapSphere(enemy.transform.position, enemy.runEndDistance * 30, enemy.playerLayer);

        player = null;

        foreach (Collider collider in cPlayer)
        {
            player = collider.GetComponentInParent<Player>();

            if (player != null) break;
        }
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
