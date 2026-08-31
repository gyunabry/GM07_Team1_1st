using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    [SerializeField] CurrencySystem currencySystem;
    public NavMeshAgent navMeshAgent;
    private int nowLevel = 0;

    // 복구 전 최초 스탯
    private float initialBaseAttackDamage;
    private float initialBaseAttackSpeed;
    private float initialBaseAttackDistance;
    private float initialMoveSpeed;

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

    private void Awake()
    {
        initialBaseAttackDamage = baseAttackDamage;
        initialBaseAttackSpeed = baseAttackSpeed;
        initialBaseAttackDistance = baseAttackDistance;
        initialMoveSpeed = moveSpeed;
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

    public void RestoreProgress(int savedCurrencyLevel, int savedSkillPoints)
    {
        skillPoint = Mathf.Max(0, savedSkillPoints);

        // 초기값으로 설정
        // 이후 레벨에 따라 보너스 스탯 적용
        baseAttackDamage = initialBaseAttackDamage;
        baseAttackSpeed = initialBaseAttackSpeed;
        baseAttackDistance = initialBaseAttackDistance;
        moveSpeed = initialMoveSpeed;

        int upgradeCount = Mathf.Clamp(savedCurrencyLevel - 1, 0, levelUpStats.Count);

        for (int i = 0; i < upgradeCount; i++)
        {
            LevelUpStat stat = levelUpStats[i];
            if (stat == null) continue;

            baseAttackDamage += stat.attackDamage;
            baseAttackSpeed += stat.attackSpeed;
            baseAttackDistance += stat.attackDistance;
            moveSpeed += stat.moveSpeed;
        }

        nowLevel = upgradeCount;

        if (navMeshAgent != null)
        {
            navMeshAgent.speed = 3.5f + moveSpeed;
        }
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
