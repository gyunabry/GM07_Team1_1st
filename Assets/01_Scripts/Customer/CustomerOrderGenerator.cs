using System.Collections.Generic;
using UnityEngine;

// 해금된 생산품을 손님 주문으로 변환한다.
public sealed class CustomerOrderGenerator
{
    private readonly RecipeUnlockManager unlockManager;

    public CustomerOrderGenerator(RecipeUnlockManager unlockManager)
    {
        this.unlockManager = unlockManager;
    }

    public bool TryCreateOrder(CustomerOrder rewardTemplate, out CustomerOrder order)
    {
        order = default;

        if (rewardTemplate.Reward < 0 || rewardTemplate.ExperienceReward < 0)
        {
            return false;
        }

        List<ItemDataSO> candidates = CollectProductCandidates();
        if (candidates.Count == 0)
        {
            return false;
        }

        ItemDataSO selectedItem = candidates[Random.Range(0, candidates.Count)];
        order = new CustomerOrder
        {
            Items = new List<CustomerOrderItem>
            {
                new CustomerOrderItem { ItemId = selectedItem, Amount = 1 }
            },
            Reward = rewardTemplate.Reward,
            ExperienceReward = rewardTemplate.ExperienceReward
        };

        return true;
    }

    private List<ItemDataSO> CollectProductCandidates()
    {
        List<ItemDataSO> candidates = new();
        HashSet<string> addedItemIds = new();

        if (unlockManager != null)
        {
            AddRecipeOutputs(unlockManager.UnlockedRecipes, candidates, addedItemIds);
            if (candidates.Count > 0)
            {
                return candidates;
            }
        }

        // 해금 관리자가 없거나 유효한 해금 레시피를 읽지 못한 경우에는,
        // 실제 배치된 생산 건물의 레시피를 주문 후보로 사용한다.
        ProductionBuilding[] productionBuildings = Object.FindObjectsByType<ProductionBuilding>(FindObjectsSortMode.None);
        for (int i = 0; i < productionBuildings.Length; i++)
        {
            ProductionBuilding building = productionBuildings[i];
            if (building != null)
            {
                AddRecipeOutputs(building.AvailableRecipes, candidates, addedItemIds);
            }
        }

        return candidates;
    }

    private static void AddRecipeOutputs(
        IReadOnlyList<RecipeDataSO> recipes,
        List<ItemDataSO> candidates,
        HashSet<string> addedItemIds)
    {
        if (recipes == null)
        {
            return;
        }

        for (int i = 0; i < recipes.Count; i++)
        {
            ItemDataSO output = recipes[i] != null ? recipes[i].Output : null;
            if (output == null || output.ItemType != ItemType.Product || string.IsNullOrEmpty(output.ItemId))
            {
                continue;
            }

            if (addedItemIds.Add(output.ItemId))
            {
                candidates.Add(output);
            }
        }
    }
}
