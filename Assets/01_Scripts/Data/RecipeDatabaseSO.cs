using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeDatabase", menuName = "Tycoon/Recipe Database")]
public class RecipeDatabaseSO : ScriptableObject
{
    [field: SerializeField]
    public List<RecipeDataSO> Items = new List<RecipeDataSO>();

    public bool TryGetById(string itemId, out RecipeDataSO result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(itemId))
        {
            return false;
        }

        foreach (RecipeDataSO data in Items)
        {
            if (data == null) continue;

            if (string.Equals(data.RecipeId, itemId, System.StringComparison.Ordinal))
            {
                result = data;
                return true;
            }
        }

        return false;
    }
}
