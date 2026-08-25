using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] Slider expSlider;
    CurrencySystem currencySystem;
    List<int> needExp = new List<int>() { 60, 110, 260, 330, 430, 800, 950, 1100, 6500, 7200, 10000, 12000, 18000, 20000, 24000, 27000,
    32000, 35000, 40000, 44000};
    public int NowLevel { get; private set; }
    public int skillPoint = 0;

    public float baseAttackDamage;
    public float baseAttackSpeed;
    public float AttackDamage;
    public float AttackSpeed;

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
        int exp = currencySystem.Experience;
        for (int i = 0; i < needExp.Count; i++)
        {
            if (exp >= needExp[i])
            {
                exp -= needExp[i];
                level++;
            }
        }
        if(exp != 0)
        {
            float expSliderValue = exp / (needExp[level] / 100);
            expSlider.value = expSliderValue;
        }
        else
        {
            expSlider.value = 0;
        }
        if (NowLevel >= level) return;
        else
        {
            skillPoint += (level - NowLevel) * 3;
            NowLevel = level;
            LevelUp?.Invoke();
        }

    }
}
