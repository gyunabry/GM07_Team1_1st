using UnityEngine;
using UnityEngine.EventSystems;

public class StorageDecomposition : MonoBehaviour
{
    [SerializeField] StorageDecompositionValuePopup valuePopup;
    [SerializeField] CurrencySystem currencySystem;
    
    public void OnClickDecompositionButton(ItemInventory sourceInventory, ItemDataSO itemData)
    {
        if (sourceInventory == null || itemData == null || valuePopup == null || currencySystem == null)
        {
            return;
        }

        int maxCount = sourceInventory.GetAmount(itemData);
        if (maxCount <= 0) return;

        valuePopup.OpenPopup(maxCount, selectedValue =>
        {
            DecompositinonItem(sourceInventory, selectedValue, itemData);
        });
    }

    private void DecompositinonItem(ItemInventory sourceInventory, int value, ItemDataSO itemData)
    {
        if (value != 0)
        {
            int totalExp = (itemData.Exp * value) / 10;
            currencySystem.GrantExperience(totalExp);

            sourceInventory.Remove(itemData, value);
        }
    }
}
