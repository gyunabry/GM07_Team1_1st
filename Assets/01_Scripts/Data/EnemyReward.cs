using System;
using UnityEngine;

[Serializable]
public struct EnemyReward
{
    [SerializeField] private ItemAmount drop;

    [SerializeField, Min(1)] private int killExp;

    public ItemAmount Drop => drop;
    public int KillExp => killExp;
}
