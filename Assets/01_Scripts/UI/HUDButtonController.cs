using UnityEngine;
using UnityEngine.UI;

public class HUDButtonController : MonoBehaviour
{
    [SerializeField] private CharacterPanelController characterPanel;
    [SerializeField] private CarrierCommandPanelView employeePanel;
    [SerializeField] private OptionUI optionUI;

    public void OpenCharacterPanel()
    {
        if (!characterPanel.IsOpen)
        {
            AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);
        }
        else
        {
            AudioManager.Instance.PlaySFX(ESFXType.UI_Close);
        }
        characterPanel.ToggleUI();
        employeePanel.Hide();
    }

    public void OpenEmployeePanel()
    {
        if (!employeePanel.IsOpen)
        {
            AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);
        }
        else
        {
            AudioManager.Instance.PlaySFX(ESFXType.UI_Close);
        }
        characterPanel.Hide();
        employeePanel.Toggle();
    }

    public void OpenOptionPanel()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);
        optionUI.Show();
    }
}
