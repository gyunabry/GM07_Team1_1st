using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CarrierCommandRowView : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private Button minusButton;
    [SerializeField] private Button plusButton;

    private ProductionBuilding building;
    private CarrierCommandType commandType;
    private CarrierCommandService commandService;
    private Func<int> totalCarrierCountProvider;
    private Action changed;

    public void Bind(
        ProductionBuilding targetBuilding,
        CarrierCommandType type,
        CarrierCommandService service,
        Func<int> totalCountProvider,
        Action onChanged)
    {
        building = targetBuilding;
        commandType = type;
        commandService = service;
        totalCarrierCountProvider = totalCountProvider;
        changed = onChanged;

        minusButton.onClick.RemoveAllListeners();
        plusButton.onClick.RemoveAllListeners();
        minusButton.onClick.AddListener(ClearOne);
        plusButton.onClick.AddListener(AssignOne);
        Refresh();
    }

    public void Refresh()
    {
        RecipeDataSO recipe = building != null ? building.SelectedRecipe : null;
        if (recipe == null || commandService == null)
        {
            gameObject.SetActive(false);
            return;
        }

        ItemDataSO item = commandType == CarrierCommandType.Material ? recipe.Input : recipe.Output;
        itemIcon.sprite = item != null ? item.Icon : null;
        itemIcon.enabled = itemIcon.sprite != null;
        itemIcon.preserveAspect = true;

        int assignedCount = commandService.GetCommandCount(commandType, building);
        int totalCount = Mathf.Max(0, totalCarrierCountProvider());
        amountText.text = assignedCount + " / " + totalCount;
        minusButton.interactable = assignedCount > 0;
        plusButton.interactable = commandService.GetAvailableWorkerCount() > 0;
        gameObject.SetActive(true);
    }

    private void AssignOne()
    {
        if (commandService.TryAssignCommand(commandType, building))
        {
            AudioManager.Instance.PlaySFX(ESFXType.UI_Comfirm);
        }
        changed?.Invoke();
    }
    
    private void ClearOne()
    {
        if (commandService.TryClearOneCommand(commandType, building))
        {
            AudioManager.Instance.PlaySFX(ESFXType.UI_Cancel);
        }
        changed?.Invoke();
    }
}
