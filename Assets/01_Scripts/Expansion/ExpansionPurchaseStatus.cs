using System;
using UnityEngine;

[Flags]
public enum ExpansionBlockReason
{
    None = 0,
    AlreadyPurchase = 1 << 0,
    PreviousExpansionRequired = 1 << 1,
    LevelRequired = 1 << 2,
    NotEnoughMoney = 1 << 3,
}

public readonly struct ExpansionPurchaseStatus
{
    public int Price { get; }
    public ExpansionBlockReason BlockReasons { get; }

    public bool CanPurchase => BlockReasons == ExpansionBlockReason.None;

    public bool HasReason(ExpansionBlockReason reason)
    {
        return (BlockReasons & reason) != 0;
    }

    public ExpansionPurchaseStatus(int price, ExpansionBlockReason blockReasons)
    {
        Price = price;
        BlockReasons = blockReasons;
    }
}
