using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "ScriptableObjects/EnemySO")]
public class EnemySO : ScriptableObject
{
    public string enemyName;
    public int hp;
    public float patrolSpeed;
    public float runSpeed;
    public List<DropItemSO> dropItem;
}
