using System.Collections.Generic;
using UnityEngine;

public class StorageSkillRegistry : MonoBehaviour
{
    private static readonly HashSet<ItemInventory> targets = new();

    private static int currentBonus;
    private static int pendingBonus;

    public static int CurrentBonus => currentBonus;

    public static void Register(ItemInventory inventory)
    {
        if (inventory == null) return;

        targets.Add(inventory);

        inventory.SetBonusCapacity(currentBonus);
    }

    public static void Unregister(ItemInventory inventory)
    {
        if (inventory != null)
        {
            targets.Remove(inventory);
        }
    }

    public static void BeginRebuild()
    {
        pendingBonus = 0;
    }

    public static void AddCapacityBonus(int amount)
    {
        pendingBonus += Mathf.Max(0, amount);
    }

    public static void Commit()
    {
        currentBonus = pendingBonus;

        foreach (ItemInventory inventory in targets)
        {
            inventory?.SetBonusCapacity(currentBonus);
        }
    }

    private static void ResetRuntimeState()
    {
        targets.Clear();
        currentBonus = 0;
        pendingBonus = 0;
    }
}
