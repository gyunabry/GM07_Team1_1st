using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

//클릭한 지점이 이동 가능한 구역일 시, 도착 Marker를 표시하고 가장 빠른 경로로 이동한다.
public class NavMeshClickMove : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform destinationMarker;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float sampleDistance = 2.0f;

    [Header("참조")]
    [SerializeField] private PlayerInteractionController interactionController;
    [SerializeField] private PlacementSystem placementSystem;
    [SerializeField] private PlayerAnimationController animationController;

    private NavMeshAgent agent;
    private NavMeshPath calculatePath;
    private Vector3 currentDestination;
    private bool hasDestination;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        calculatePath = new NavMeshPath();
        destinationMarker.gameObject.SetActive(false);
    }

    private void Update() //(최적화 고려 : 코루틴, 이벤트 등)
    {
        MouseInput();
        UpdateAnimation();
        UpdateMarker();
    }

    private void MouseInput()
    {
        if (Mouse.current == null) return;

        bool leftClicked = Mouse.current.leftButton.wasPressedThisFrame;
        bool rightClicked = Mouse.current.rightButton.wasPressedThisFrame;

        if (!leftClicked && !rightClicked) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (leftClicked)
        {
            if (placementSystem != null && placementSystem.IsPlacementMode)
            {
                return;
            }

            interactionController?.TryInteractUnderPointer();
            return;
        }

        //if (!Mouse.current.rightButton.wasPressedThisFrame) return;

        //if (interactionController != null && interactionController.TryInteractUnderPointer())
        //{
        //    return;
        //}

        //if (placementSystem != null && placementSystem.IsPlacementMode)
        //{
        //    return;
        //}

        SetDestination();
    }

    //Ray로 바닥을 찾고, 클릭 위치 주변 NavMesh 찾은 뒤, 경로 계산
    private void SetDestination()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(mousePosition.x, mousePosition.y, 0.0f));
        RaycastHit hit;

        bool hitGround = Physics.Raycast(ray, out hit, 100.0f, groundMask, QueryTriggerInteraction.Ignore);
        if (!hitGround) return;
        NavMeshHit navMeshHit;

        bool foundPosition = NavMesh.SamplePosition(hit.point, out navMeshHit, sampleDistance, agent.areaMask);
        if (!foundPosition) return;

        CalculateApplyPath(navMeshHit.position);
    }

    //현재 위치에서 목적지까지 경로 계산, 완전한 경로일 시 목적지 좌표와 마커 갱신
    private void CalculateApplyPath(Vector3 destination)
    {
        bool foundPath = agent.CalculatePath(destination, calculatePath);
        if (!foundPath || calculatePath.status != NavMeshPathStatus.PathComplete)
        {
            agent.ResetPath();
            hasDestination = false;
            return;
        }
        currentDestination = destination;
        hasDestination = true;
        agent.SetPath(calculatePath);
        destinationMarker.gameObject.SetActive(true);
        destinationMarker.position = destination + Vector3.up * 0.05f;
    }

    //(예비 구현)비용 변경 시, 마지막 목적지까지 새로운 경로 계산
    //비용 변경 기능 구현 필요 시 신규 Script 생성 후 구현.
    public void ReCalculateCurrentPath()
    {
        if (hasDestination) return;
        if (agent.isOnOffMeshLink) return;

        CalculateApplyPath(currentDestination);
    }

    //캐릭터 애니메이션
    private void UpdateAnimation()
    {
        if (animationController == null) return;

        PlayerAnimationController.PlayerAnimState nextState;

        if(agent.velocity.magnitude == 0)
        {
            nextState = PlayerAnimationController.PlayerAnimState.Idle;
        }
        else
        {
            nextState = PlayerAnimationController.PlayerAnimState.Walk;
        }
        animationController.SetState(nextState);
    }

    // 목적지 마커 갱신
    private void UpdateMarker()
    {
        if (!hasDestination) return;

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            hasDestination = false;
            destinationMarker.gameObject.SetActive(false);
            return;
        }

        bool cannotMove = agent.isStopped && !agent.isOnOffMeshLink;

        if (cannotMove)
        {
            hasDestination = false;
            destinationMarker.gameObject.SetActive(false);
            return;
        }

        // 경로 계산 완료 전까지는 도착 여부 판단 X
        if (agent.pathPending) return;

        if (!agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            hasDestination = false;
            destinationMarker.gameObject.SetActive(false);
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            hasDestination = false;
            destinationMarker.gameObject.SetActive(false);
        }
    }
}
