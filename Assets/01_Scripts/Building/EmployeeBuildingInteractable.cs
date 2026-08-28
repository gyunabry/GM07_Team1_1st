using UnityEngine;

/// <summary>
/// </summary>
public sealed class EmployeeBuildingInteractable : InteractableBase
{
    public override void Interact(GameObject interactor)
    {
        PlayerInteractionController interactionController =
            interactor.GetComponent<PlayerInteractionController>();

        if (interactionController == null)
        {
            return;
        }

        interactionController.OpenEmployeeUI(this);
    }
}
