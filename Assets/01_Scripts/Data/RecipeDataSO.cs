using UnityEngine;
 
public enum ProcessType
{
    Refine, // 정제
    Heat    // 가열
}

[CreateAssetMenu(fileName = "RecipeData", menuName = "Tycoon/Recipe Data")]
public class RecipeDataSO : ScriptableObject
{
    [SerializeField] private string recipeId;
    [SerializeField] private string recipeName;

    [SerializeField] private ProcessType processType;
    [SerializeField] private ItemDataSO input;
    [SerializeField] private ItemDataSO output;

    [SerializeField] private float productionTime;

    public string RecipeId => recipeId;
    public string RecipeName => recipeName;

    public ProcessType ProcessType => processType;
    public ItemDataSO Input => input;
    public ItemDataSO Output => output;

    public float ProductionTime => productionTime;
}