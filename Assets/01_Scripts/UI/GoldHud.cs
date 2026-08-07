using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GoldHud : MonoBehaviour
{
    private CurrencySystem currencySystem;
    private TextMeshProUGUI text;

    private void Awake()
    {
        currencySystem = FindAnyObjectByType<CurrencySystem>();
        text = GetComponent<TextMeshProUGUI>();
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
        text.text = $"Gold : {arg1}";
    }
}
