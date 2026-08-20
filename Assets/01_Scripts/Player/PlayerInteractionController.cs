using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerInteractionController : MonoBehaviour, IBuildingUIOpener
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask interactionLayer;
    [SerializeField] private PlayerInteractionDetector detector;
    [SerializeField] private BuildingUIRouter buildingUIRouter;

    private IInteractable activeInteractable;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (detector == null)
        {
            detector = GetComponentInChildren<PlayerInteractionDetector>();
        }
    }

    private void OnEnable()
    {
        if (detector != null)
        {
            detector.InteractableExited += HandleInteractableExited;
        }
    }

    private void OnDisable()
    {
        if (detector != null)
        {
            detector.InteractableExited -= HandleInteractableExited;
        }
    }

    // 마우스가 가리키는 상호작용 대상을 찾아 실행
    public bool TryInteractUnderPointer()
    {
        if (mainCamera == null || detector == null) return false;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }

        Vector2 mousePos = Mouse.current.position.ReadValue();

        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, interactionLayer, QueryTriggerInteraction.Collide))
        {
            return false;
        }

        IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

        if (interactable == null) return false;

        // 클릭한 건물이 플레이어 감지 범위 밖에 있으면 false 리턴
        if (!detector.Contains(interactable)) return false;

        GameObject interactor = transform.root.gameObject;

        // 해당 건물이 상호작용 가능한 상태가 아니라면 false 리턴
        if (!interactable.CanInteract(interactor))
        {
            return false;
        }

        interactable.Interact(interactor);

        return true;
    }

    public void OpenBuildingUI(IBuildingUIModel building)
    {
        if (building == null) return;

        // 시설 UI를 열때 현재 시설로 저장
        if (building is Component buildingComponent)
        {
            activeInteractable = buildingComponent.GetComponent<IInteractable>();
        }

        buildingUIRouter.Open(building);
    }

    private void HandleInteractableExited(IInteractable exitedInteractable)
    {
        if (!ReferenceEquals(activeInteractable, exitedInteractable))
        {
            return;
        }

        buildingUIRouter.Close();
        activeInteractable = null;
    }
}
