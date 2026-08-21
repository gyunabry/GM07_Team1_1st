using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;

// 포탈 양쪽에 추가해 양방향 이동 지원
// 추후에는 공방 내 포탈과 상호작용 시 이동할 사냥터를 고르는 UI와 연결 예정

public class Portal : InteractableBase
{
    [Header("포탈 연결")]
    [SerializeField] private Portal linkedPortal1;
    [SerializeField] private Portal linkedPortal2;
    [SerializeField] private Portal linkedPortal3;
    [SerializeField] private Portal linkedPortal4;

    [Tooltip("플레이어가 포탈을 탔을 때 도착할 위치")]
    [SerializeField] private Transform arrivalPoint;

    [SerializeField] private float navMeshSampleDistance = 1.5f;

    [Tooltip("포탈 UI")]
    [SerializeField] private Canvas portalUI;

    [SerializeField] private UnityEvent AttackOn;
    [SerializeField] private UnityEvent AttackOff;

    Transform target;

    // 반대 포탈에서 해당 포탈에 접근하기 위한 프로퍼티
    private Transform ArrivalPoint => arrivalPoint != null ? arrivalPoint : null;

    public override void Interact(GameObject interactor)
    {
        portalUI.gameObject.SetActive(true);
        Button[] teleportButton = portalUI.GetComponentsInChildren<Button>();
        teleportButton[0].onClick.AddListener(() => SelectPortal(interactor, 1));
        teleportButton[1].onClick.AddListener(() => SelectPortal(interactor, 2));
        teleportButton[2].onClick.AddListener(() => SelectPortal(interactor, 3));
        teleportButton[3].onClick.AddListener(() => SelectPortal(interactor, 4));
    }
    public void SelectPortal(GameObject interactor, int portalList)
    {
        if (interactor == null || linkedPortal1 == null || linkedPortal2 == null || linkedPortal3 == null || linkedPortal4 == null)
        {
            return;
        }

        NavMeshAgent agent = interactor.GetComponent<NavMeshAgent>();

        if (agent == null || !agent.enabled)
        {
            Debug.LogWarning("이동 가능한 NavMeshAgent가 없습니다.");
            return;
        }
        
        switch (portalList)
        {
            case 1: target = linkedPortal1.ArrivalPoint;break;
            case 2: target = linkedPortal2.ArrivalPoint;break;
            case 3: target = linkedPortal3.ArrivalPoint;break;
            case 4: target = linkedPortal4.ArrivalPoint;break;
        }
        

        agent.ResetPath();

        agent.Warp(target.position);

        // 에이전트의 회전 방향을 포탈이 바라보는 방향과 일치
        agent.transform.rotation = target.rotation;

        if(portalList != 1)
        {
            AttackOn?.Invoke();
        }
        else
        {
            AttackOff?.Invoke();
        }

            portalUI.gameObject.SetActive(false);
    }
}
 