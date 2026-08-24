using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;

// 포탈 양쪽에 추가해 양방향 이동 지원
// 추후에는 공방 내 포탈과 상호작용 시 이동할 사냥터를 고르는 UI와 연결 예정

public enum PortalInteractionMode
{
    WorkshopSelector,   // 공방에서 포탈 상호작용
    HuntingFieldReturn  // 사냥터에서 포탈 상호작용
}

public class Portal : InteractableBase
{
    // 실제 사냥터 개수
    // 추후 해당 부분 수정해서 포탈 연결
    private const int HuntingFieldCount = 3;

    [Header("포탈 연결")]
    //[SerializeField] private Portal linkedPortal1;
    //[SerializeField] private Portal linkedPortal2;
    //[SerializeField] private Portal linkedPortal3;
    //[SerializeField] private Portal linkedPortal4;
    [SerializeField] private PortalInteractionMode interactionMode;

    [Header("도착 설정")]
    [Tooltip("플레이어가 포탈을 탔을 때 도착할 위치")]
    [SerializeField] private Transform arrivalPoint;
    //[Tooltip("도착지점 주변에서 NavMesh를 검색할 거리")]
    //[SerializeField] private float navMeshSampleDistance = 1.5f;

    [Header("공방 포탈 설정")]
    [SerializeField] private TeleportUI teleportUI;
    [Tooltip("각 사냥터에 대응하는 포탈")]
    [SerializeField] private Portal[] huntingFieldPortals = new Portal[HuntingFieldCount];

    [Header("사냥터 포탈 설정")]
    [SerializeField] private Portal workshopPortal;

    //[Tooltip("포탈 UI")]
    //[SerializeField] private Canvas portalUI;

    [Header("도착 이벤트")]
    [Tooltip("플레이어가 포탈에 도착한 뒤 실행할 이벤트")]
    [SerializeField] private UnityEvent onArrived = new UnityEvent();
    //[SerializeField] private UnityEvent AttackOn;
    //[SerializeField] private UnityEvent AttackOff;

    Transform target;

    // 반대 포탈에서 해당 포탈에 접근하기 위한 프로퍼티
    private Transform ArrivalPoint => arrivalPoint != null ? arrivalPoint : null;

    public override void Interact(GameObject interactor)
    {
        if (interactor == null)
        {
            Debug.LogWarning($"{name} 상호작용 대상이 없습니다.");
            return;
        }

        switch (interactionMode)
        {
            case PortalInteractionMode.WorkshopSelector:
                OpenSelectionUI(interactor);
                break;

            case PortalInteractionMode.HuntingFieldReturn:
                TryTeleportTo(interactor, workshopPortal);
                break;
        }

        //portalUI.gameObject.SetActive(true);
        //Button[] teleportButton = portalUI.GetComponentsInChildren<Button>();
        //teleportButton[0].onClick.AddListener(() => SelectPortal(interactor, 1));
        //teleportButton[1].onClick.AddListener(() => SelectPortal(interactor, 2));
        //teleportButton[2].onClick.AddListener(() => SelectPortal(interactor, 3));
        //teleportButton[3].onClick.AddListener(() => SelectPortal(interactor, 4));
    }

    private void OpenSelectionUI(GameObject interactor)
    {
        if (teleportUI == null)
        {
            Debug.LogWarning($"{name} 텔레포트 UI가 연결되지 않았습니다.");
            return;
        }

        teleportUI.OpenUI(this, interactor, huntingFieldPortals);
    }

    public bool TryTeleportTo(GameObject interactor, Portal destination)
    {
        if (interactor == null)
        {
            Debug.LogWarning($"{name} 이동시킬 대상이 없습니다.");
            return false;
        }

        if (destination == null)
        {
            Debug.LogWarning($"{name} 목적지 포탈이 연결되지 않았습니다.");
            return false;
        }

        return destination.TryReceive(interactor);
    }

    private bool TryReceive(GameObject interactor)
    {
        if (arrivalPoint == null )
        {
            Debug.LogWarning($"{name} 목적지점이 없습니다.");
            return false;
        }

        NavMeshAgent agent = interactor.GetComponent<NavMeshAgent>();

        if (agent == null || !agent.enabled)
        {
            Debug.LogWarning($"{name} 활성화된 NavMeshAgent가 없습니다.");
            return false;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"{name} NavMesh 위에 있지 않습니다.");
            return false;
        }

        agent.ResetPath();
        if (!agent.Warp(arrivalPoint.position))
        {
            Debug.LogWarning($"{interactor.name} {name}으로 이동하지 못했습니다.");
            return false;
        }

        agent.transform.rotation = arrivalPoint.rotation;

        // 이동이 성공했을 때 공격 상태 전환
        onArrived?.Invoke();

        return true;
    }

    //public void SelectPortal(GameObject interactor, int portalList)
    //{
    //    if (interactor == null || linkedPortal1 == null || linkedPortal2 == null || linkedPortal3 == null || linkedPortal4 == null)
    //    {
    //        return;
    //    }

    //    NavMeshAgent agent = interactor.GetComponent<NavMeshAgent>();

    //    if (agent == null || !agent.enabled)
    //    {
    //        Debug.LogWarning("이동 가능한 NavMeshAgent가 없습니다.");
    //        return;
    //    }
        
    //    switch (portalList)
    //    {
    //        case 1: target = linkedPortal1.ArrivalPoint;break;
    //        case 2: target = linkedPortal2.ArrivalPoint;break;
    //        case 3: target = linkedPortal3.ArrivalPoint;break;
    //        case 4: target = linkedPortal4.ArrivalPoint;break;
    //    }
        

    //    agent.ResetPath();

    //    agent.Warp(target.position);

    //    // 에이전트의 회전 방향을 포탈이 바라보는 방향과 일치
    //    agent.transform.rotation = target.rotation;

    //    if(portalList != 1)
    //    {
    //        AttackOn?.Invoke();
    //    }
    //    else
    //    {
    //        AttackOff?.Invoke();
    //    }

    //        portalUI.gameObject.SetActive(false);
    //}
}
 