using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeUnlockConfig", menuName = "Tycoon/Recipe Unlock Config")]
public sealed class RecipeUnlockConfigSO : ScriptableObject
{
    [SerializeField] private List<RecipeDataSO> initialUnlockedRecipes = new();

    public IReadOnlyList<RecipeDataSO> InitialUnlockedRecipes => initialUnlockedRecipes;
}
