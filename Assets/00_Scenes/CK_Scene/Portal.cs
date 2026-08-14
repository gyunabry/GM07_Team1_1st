using UnityEngine;
using UnityEngine.AI;

// 포탈 양쪽에 추가해 양방향 이동 지원
// 추후에는 공방 내 포탈과 상호작용 시 이동할 사냥터를 고르는 UI와 연결 예정

public class Portal : InteractableBase
{
    [Header("포탈 연결")]
    [SerializeField] private Portal linkedPortal;

    [Tooltip("플레이어가 포탈을 탔을 때 도착할 위치")]
    [SerializeField] private Transform arrivalPoint;

    [SerializeField] private float navMeshSampleDistance = 1.5f;

    // 반대 포탈에서 해당 포탈에 접근하기 위한 프로퍼티
    private Transform ArrivalPoint => arrivalPoint != null ? arrivalPoint : null;

    public override void Interact(GameObject interactor)
    {
        if (interactor == null || linkedPortal == null)
        {
            return;
        }

        NavMeshAgent agent = interactor.GetComponent<NavMeshAgent>();

        if (agent == null || !agent.enabled)
        {
            Debug.LogWarning("이동 가능한 NavMeshAgent가 없습니다.");
            return;
        }
        
        Transform target = linkedPortal.ArrivalPoint;

        agent.ResetPath();
        
        agent.Warp(target.position);

        // 에이전트의 회전 방향을 포탈이 바라보는 방향과 일치
        agent.transform.rotation = target.rotation;
    }
}
 