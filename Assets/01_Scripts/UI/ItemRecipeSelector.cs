using UnityEngine;
using UnityEngine.UI;

public class ItemRecipeSelector : MonoBehaviour
{
    [SerializeField] private Button itemButton;
    [SerializeField] private GameObject recipeSelectPanel;

    private bool isEnabled = false;

    private void Awake()
    {
        HideRecipePanel();
    }

    private void Start()
    {
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(ToggleRecipePanel);
        }
    }

    public void ToggleRecipePanel()
    {
        if (recipeSelectPanel != null)
        {
            isEnabled = !isEnabled;

            recipeSelectPanel.SetActive(isEnabled);
        }
    }

    // 시작, 레시피 선택 완료시 호출해 해당 패널을 비활성화
    public void HideRecipePanel()
    {
        if (recipeSelectPanel != null)
        {
            recipeSelectPanel.SetActive(false);
        }
    }
}
