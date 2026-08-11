using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    CurrencySystem currencySystem;
    List<int> needExp = new List<int>() {0, 50, 100, 150, 200 };
    public int NowLevel { get; private set;}
    public int skillPoint;

    public float baseAttackDamage = 0;
    public float baseAttackSpeed = 0;
    public float AttackDamage = 0;
    public float AttackSpeed = 0;

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
        
    }
}
