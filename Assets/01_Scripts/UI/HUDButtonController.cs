using UnityEngine;
using UnityEngine.UI;

public class HUDButtonController : MonoBehaviour
{
    [SerializeField] private CharacterPanelController characterPanel;
    [SerializeField] private CarrierCommandPanelView employeePanel;

    public void OpenCharacterPanel()
    {
        characterPanel.ToggleUI();
        employeePanel.Hide(); ;
    }

    public void OpenEmployeePanel()
    {
        characterPanel.Hide();
        employeePanel.Toggle();
    }
}
