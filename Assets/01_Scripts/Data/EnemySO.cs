using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct EnemyReward
{
    [SerializeField] private ItemAmount drop;

    [SerializeField, Min(1)] private int killExp;

    public ItemAmount Drop => drop;
    public int KillExp => killExp;
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "Tycoon/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    //public int enemyCode;
    //public string enemyName;
    //public int hp;
    //public float patrolSpeed;
    //public float runSpeed;
    //public float runStartDis;
    //public float runEndDis;
    //public List<DropItemSO> dropItem;

    [Header("기본 정보")]
    [field: SerializeField]
    public string EnemyId { get; private set; }

    [field: SerializeField]
    public string EnemyName { get; private set; }

    [field: SerializeField]
    public string Description { get; private set; }

    [Header("능력치")]
    [field: SerializeField]
    public int Hp { get; private set; }

    [field: SerializeField]
    public float PatrolSpeed { get; private set; }

    [field: SerializeField]
    public float RunSpeed { get; private set; }

    [field: SerializeField]
    public float RunStartDistance { get; private set; }

    [field: SerializeField]
    public float RunEndDistance { get; private set; }

    [Header("처치 보상")]
    [field: SerializeField]
    public EnemyReward Reward { get; private set; }
}
