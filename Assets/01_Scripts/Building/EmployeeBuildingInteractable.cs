using UnityEngine;

/// <summary>
/// 직원 건물(사냥꾼/운반꾼)을 클릭하면 직원 정보를 표시합니다.
/// </summary>
public sealed class EmployeeBuildingInteractable : InteractableBase
{
    [SerializeField] private GameObject employeeUIPrefab;

    public override void Interact(GameObject interactor)
    {
        PlayerInteractionController interactionController =
            interactor.GetComponent<PlayerInteractionController>();

        if (interactionController == null)
        {
            Debug.LogWarning("직원 건물 상호작용에 PlayerInteractionController가 필요합니다.", this);
            return;
        }

        interactionController.OpenEmployeeUI(employeeUIPrefab, this);
    }
}
