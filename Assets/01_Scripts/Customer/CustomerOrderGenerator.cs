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

        if (unlockManager == null || rewardTemplate.Reward < 0 || rewardTemplate.ExperienceReward < 0)
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

        foreach (RecipeDataSO recipe in unlockManager.UnlockedRecipes)
        {
            ItemDataSO output = recipe != null ? recipe.Output : null;
            if (output == null || output.ItemType != ItemType.Product || string.IsNullOrEmpty(output.ItemId))
            {
                continue;
            }

            if (addedItemIds.Add(output.ItemId))
            {
                candidates.Add(output);
            }
        }

        return candidates;
    }
}
