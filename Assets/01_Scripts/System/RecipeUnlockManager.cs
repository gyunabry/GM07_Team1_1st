using System;
using System.Collections.Generic;
using UnityEngine;

// 레시피 해금 상태의 단일 기준점. 초기 목록은 RecipeUnlockConfigSO 에셋에서 설정한다.
// 씬 설정 방법:
// 1. 실제 게임을 실행하는 씬의 기존 활성 게임 오브젝트에 RecipeUnlockManager 컴포넌트를 추가한다. 활성 씬에는 하나만 존재해야 한다.
// 2. Inspector의 Unlock Config에 RecipeUnlockConfig.asset을 연결한다.
// 아직 해금 획득 기능은 없으며, 이후 스킬·퀘스트 등이 Unlock(RecipeDataSO)를 호출해 연동한다.
public sealed class RecipeUnlockManager : MonoBehaviour
{
    [SerializeField] private RecipeUnlockConfigSO unlockConfig;

    private readonly List<RecipeDataSO> unlockedRecipes = new();
    private readonly HashSet<RecipeDataSO> unlockedRecipeSet = new();

    public static RecipeUnlockManager Instance { get; private set; }
    public IReadOnlyList<RecipeDataSO> UnlockedRecipes => unlockedRecipes;

    public event Action UnlockedRecipesChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Only one RecipeUnlockManager can be active.", this);
            enabled = false;
            return;
        }

        Instance = this;
        InitializeUnlockedRecipes();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool IsUnlocked(RecipeDataSO recipe)
    {
        return recipe != null && unlockedRecipeSet.Contains(recipe);
    }

    // 특정 레시피를 해금한다. 해당 레시피의 생산품은 손님 주문 후보와 생산 UI에 반영된다.
    public bool Unlock(RecipeDataSO recipe)
    {
        if (recipe == null || !unlockedRecipeSet.Add(recipe))
        {
            return false;
        }

        unlockedRecipes.Add(recipe);
        UnlockedRecipesChanged?.Invoke();
        return true;
    }

    public bool Lock(RecipeDataSO recipe)
    {
        if (recipe == null || !unlockedRecipeSet.Remove(recipe))
        {
            return false;
        }

        unlockedRecipes.Remove(recipe);
        UnlockedRecipesChanged?.Invoke();
        return true;
    }

    private void InitializeUnlockedRecipes()
    {
        unlockedRecipes.Clear();
        unlockedRecipeSet.Clear();

        if (unlockConfig == null)
        {
            Debug.LogWarning("RecipeUnlockManager requires a RecipeUnlockConfigSO.", this);
            return;
        }

        foreach (RecipeDataSO recipe in unlockConfig.InitialUnlockedRecipes)
        {
            if (recipe != null && unlockedRecipeSet.Add(recipe))
            {
                unlockedRecipes.Add(recipe);
            }
        }
    }

    public void RestoreUnlockedRecipes(IReadOnlyList<RecipeDataSO> restoredRecipes)
    {
        unlockedRecipes.Clear();
        unlockedRecipeSet.Clear();

        if (restoredRecipes != null)
        {
            foreach (RecipeDataSO recipe in restoredRecipes)
            {
                if (recipe != null && unlockedRecipeSet.Add(recipe))
                {
                    unlockedRecipes.Add(recipe);
                }
            }
        }

        UnlockedRecipesChanged?.Invoke();
    }
}
