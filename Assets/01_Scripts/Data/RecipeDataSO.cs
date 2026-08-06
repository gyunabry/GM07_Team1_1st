using System;
using UnityEngine;
using System.Collections.Generic;

public enum ProcessType
{
    Refine, // 정제
    Heat    // 가열
}

[CreateAssetMenu(fileName = "RecipeData", menuName = "Tycoon/Recipe Data")]
public class RecipeDataSO : ScriptableObject
{
    [field: SerializeField]
    public string RecipeId { get; private set; }

    [field: SerializeField]
    public string RecipeName { get; private set; } // 생산 아이템 이름과 동일

    [field: SerializeField]
    public ProcessType ProcessType { get; private set; }

    [field: SerializeField]
    public List<ItemAmount> Ingredients { get; private set; } // 재료 아이템 리스트

    [field: SerializeField]
    public float ProductionTime { get; private set; }
}
