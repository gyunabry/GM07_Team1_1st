using UnityEngine;

public class BuildingInteractable : InteractableBase
{
    private PlacedBuilding building;

    private void Awake()
    {
        building = GetComponent<PlacedBuilding>();
    }

    public override void Interact(GameObject interactor)
    {
        IBuildingUIOpener uiOpener = interactor.GetComponent<IBuildingUIOpener>();

        uiOpener?.OpenBuildingUI(building);
    }
}
