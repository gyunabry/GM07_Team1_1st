using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] CurrencySystem currencySystem;
    public NavMeshAgent navMeshAgent;
    private int nowLevel = 0;
    public int NowLevel => nowLevel;
    public int skillPoint = 0;

    public float baseAttackDamage;
    public float baseAttackSpeed;
    public float baseAttackDistance;
    public float attackDamage;
    public float attackSpeed;
    public float attackDistance;
    public float moveSpeed;

    public List<LevelUpStat> levelUpStats = new List<LevelUpStat>();

    public event Action LevelUp;
    
    private void OnEnable()
    {
        currencySystem.LevelUp += CurrencySystem_LevelUp;
        navMeshAgent = GetComponent<NavMeshAgent>();
    }
    private void OnDisable()
    {
        currencySystem.LevelUp -= CurrencySystem_LevelUp;
    }
    private void CurrencySystem_LevelUp()
    {
        if (levelUpStats[nowLevel] != null)
        {
            baseAttackDamage += levelUpStats[nowLevel].attackDamage;
            baseAttackSpeed += levelUpStats[nowLevel].attackSpeed;
            baseAttackDistance += levelUpStats[nowLevel].attackDistance;
            moveSpeed += levelUpStats[nowLevel].moveSpeed;
            navMeshAgent.speed = (3.5f + moveSpeed);
        }
        nowLevel++;
    }
}
[System.Serializable]
public class LevelUpStat
{
    public float attackDamage = 0;
    public float attackSpeed = 0;
    public float attackDistance = 0;
    public float moveSpeed = 0;
}
