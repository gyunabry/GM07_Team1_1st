using System;
using UnityEngine;

// 아이템 드랍 및 레시피에 함께 사용될 구조체

[Serializable]
public struct ItemAmount
{
    [SerializeField] private ItemDataSO item;

    [SerializeField, Min(1)] private int amount;

    public ItemDataSO Item => item;
    public int Amount => amount;

    public ItemAmount(ItemDataSO item, int amount)
    {
        this.item = item;
        this.amount = Mathf.Max(1, amount);
    }

    public bool IsValid => item != null && amount > 0;
}
