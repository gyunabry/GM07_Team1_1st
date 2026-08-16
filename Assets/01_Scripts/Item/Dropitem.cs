using UnityEngine;

public class Dropitem : MonoBehaviour, ICollectable
{
    [field: SerializeField]
    public ItemDataSO Item { get; private set; }
    public int Amount { get; private set; } = 1;

    public void Initialize(ItemDataSO item, int amount = 1)
    {
        Item = item;
        Amount = amount;
    }

    public bool TryCollect(ItemInventory target)
    {
        if (target == null || Item == null || Amount <= 0)
        {
            return false;
        }

        int added = target.Add(Item, Amount);
        if (added <= 0) return false;

        Amount -= added;
        if (Amount <= 0)
        {
            Item = null;
            Amount = 0;
            MonsterPoolManager.Instance.ReturnPool(this);
        }

        return true;
    }
}
