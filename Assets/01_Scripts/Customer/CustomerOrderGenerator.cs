using System.Collections.Generic;
using UnityEngine;

// 해금된 생산품을 손님 주문으로 변환한다.
public sealed class CustomerOrderGenerator
{
    private readonly RecipeUnlockManager unlockManager;
    private readonly float refineWeight;
    private readonly float heatWeight;

    public CustomerOrderGenerator(RecipeUnlockManager unlockManager, float refineWeight = 0.8f, float heatWeight = 0.2f)
    {
        this.unlockManager = unlockManager;
        this.refineWeight = Mathf.Max(0f, refineWeight);
        this.heatWeight = Mathf.Max(0f, heatWeight);
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

        ItemDataSO selectedItem = SelectWeightedCandidate(candidates);
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

    private ItemDataSO SelectWeightedCandidate(IReadOnlyList<ItemDataSO> candidates)
    {
        int refineCandidateCount = 0;
        int heatCandidateCount = 0;
        for (int i = 0; i < candidates.Count; i++)
        {
            if (candidates[i].ProcessType == ProcessType.Refine)
            {
                refineCandidateCount++;
            }
            else
            {
                heatCandidateCount++;
            }
        }

        float availableRefineWeight = refineCandidateCount > 0 ? refineWeight : 0f;
        float availableHeatWeight = heatCandidateCount > 0 ? heatWeight : 0f;
        float totalWeight = availableRefineWeight + availableHeatWeight;

        // 모든 타입의 가중치가 0이면 기존처럼 균등 확률로 선택한다.
        if (totalWeight <= 0f)
        {
            return candidates[Random.Range(0, candidates.Count)];
        }

        ProcessType selectedProcessType = Random.value * totalWeight < availableRefineWeight
            ? ProcessType.Refine
            : ProcessType.Heat;

        int selectedCandidateIndex = Random.Range(
            0,
            selectedProcessType == ProcessType.Refine ? refineCandidateCount : heatCandidateCount);

        for (int i = 0; i < candidates.Count; i++)
        {
            if (candidates[i].ProcessType != selectedProcessType)
            {
                continue;
            }

            if (selectedCandidateIndex-- == 0)
            {
                return candidates[i];
            }
        }

        return candidates[candidates.Count - 1];
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
