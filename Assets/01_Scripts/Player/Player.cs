using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    CurrencySystem currencySystem;
    List<int> needExp = new List<int>() {50, 150, 300, 450, 650, 900, 1200, 1550, 2000, 2500, 3050, 3600, 4200, 4850, 5550, 6300};
    public int NowLevel { get; private set;}
    public int skillPoint = 0;

    public float baseAttackDamage = 0;
    public float baseAttackSpeed = 0;
    public float AttackDamage = 0;
    public float AttackSpeed = 0;

    public event Action LevelUp;

    private void Awake()
    {
        currencySystem = FindAnyObjectByType<CurrencySystem>();
    }
    private void OnEnable()
    {
        currencySystem.CurrencyChanged += CurrencySystem_CurrencyChanged;
    }
    private void OnDisable()
    {
        currencySystem.CurrencyChanged -= CurrencySystem_CurrencyChanged;
    }

    private void CurrencySystem_CurrencyChanged(int arg1, int arg2)
    {
        int level = 0;
        for (int i = 0; i < needExp.Count; i++) 
        { 
            if(currencySystem.Experience >= needExp[i])
            {
                level++;
            }
        }
        if (NowLevel >= level) return;
        else
        {
            skillPoint += level - NowLevel;
            NowLevel = level;
            LevelUp?.Invoke();
        }
    }
}
