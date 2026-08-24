using UnityEngine;

public class Dropitem : MonoBehaviour, ICollectable
{
    [SerializeField] private SpriteRenderer sr;

    public ItemDataSO Item { get; private set; }
    public int Amount { get; private set; } = 1;

    public void Initialize(ItemDataSO item, int amount = 1)
    {
        Item = item;
        Amount = amount;

        if (sr != null)
        {
            sr.sprite = item != null ? item.Icon : null;
            sr.enabled = item != null && item.Icon != null;
        }
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
            PoolManager.Instance.ReturnPool(this);
        }

        return true;
    }

    private void OnDisable()
    {
        Item = null;
        Amount = 0;

        if (sr != null)
        {
            sr.sprite = null;
            sr.enabled = false;
        }
    }
}
