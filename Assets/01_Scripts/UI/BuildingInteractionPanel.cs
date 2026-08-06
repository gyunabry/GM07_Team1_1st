using TMPro;
using UnityEngine;

public class BuildingInteractionPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text buildingNameText;

    private void Awake()
    {
        HidePanel();
    }

    public void ShowPanel(IBuildingUIModel building)
    {
        if (building == null) return;

        buildingNameText.text = building.BuildingName;
        panelRoot.SetActive(true);
    }

    public void HidePanel()
    {
        panelRoot.SetActive(false);
    }
}
