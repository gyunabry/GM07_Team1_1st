using System;
using UnityEngine;

// 몬스터의 드랍 정보를 담을 데이터

[Serializable]
public class DropEntry
{
    [field: SerializeField]
    public ItemDataSO Item { get; private set; }

    [field: SerializeField, Range(0f, 1f)]
    public float DropChance { get; private set; } = 1f;

    [field: SerializeField, Min(1)]
    public int MinAmount { get; private set; } = 1;

    [field: SerializeField, Min(1)]
    public int MaxAmount { get; private set; } = 1;
}
