using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeDatabase", menuName = "Tycoon/Recipe Database")]
public class RecipeDatabaseSO : ScriptableObject
{
    [field: SerializeField]
    public List<RecipeDataSO> Items = new List<RecipeDataSO>();
}
