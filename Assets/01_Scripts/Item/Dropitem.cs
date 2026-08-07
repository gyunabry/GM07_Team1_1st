using UnityEngine;

public class Dropitem : MonoBehaviour
{
    [field: SerializeField]
    public ItemDataSO Item { get; private set; }
    public int Amount { get; private set; } = 1;

    public void Initialize(ItemDataSO item, int amount = 1)
    {
        Item = item;
        Amount = amount;
    }

    public void Collect()
    {
        MonsterPoolManager.Instance.ReturnPool(this);
    }
}
