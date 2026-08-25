using UnityEngine;
using UnityEngine.EventSystems;

public class StorageDecomposition : MonoBehaviour
{
    [SerializeField] StorageDecompositionValuePopup valuePopup;
    [SerializeField] CurrencySystem currencySystem;
    [SerializeField] CounterInventory inventory;
    
    public void OnClickDecompositionButton(int itemValue, ItemDataSO itemData)
    {
        valuePopup.OpenPopup(itemValue, (selectedValue) =>
        {
            DecompositinonItem(selectedValue, itemData);
        });
    }
    private void DecompositinonItem(int value, ItemDataSO itemData)
    {
        if(value != 0)
        {
            int totalExp = (itemData.Exp * value) / 100;
            currencySystem.GrantExperience(totalExp);
            inventory.Inventory.Remove(itemData, value);
        }
    }
}
