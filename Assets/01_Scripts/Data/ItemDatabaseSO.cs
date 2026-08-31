using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Tycoon/Item Database")]
public class ItemDatabaseSO : ScriptableObject
{
    [field: SerializeField]
    public List<ItemDataSO> Items = new List<ItemDataSO>();

    public bool TryGetById(string itemId, out ItemDataSO result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        foreach (ItemDataSO data in Items)
        {
            if (data == null) continue;

            if (string.Equals(data.ItemId, itemId, System.StringComparison.Ordinal))
            {
                result = data;
                return true;
            }
        }

        return false;
    }
}
