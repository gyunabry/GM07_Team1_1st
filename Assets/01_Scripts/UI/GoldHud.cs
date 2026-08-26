using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GoldHud : MonoBehaviour
{
    private CurrencySystem currencySystem;
    private TextMeshProUGUI text;

    private Vector3 originScale;

    private void Awake()
    {
        currencySystem = FindAnyObjectByType<CurrencySystem>();
        text = GetComponent<TextMeshProUGUI>();
    }
    private void OnEnable()
    {
        currencySystem.CurrencyChanged += CurrencySystem_CurrencyChanged;
        currencySystem.CurrencyChanged += AddGold;
        currencySystem.CurrencyChanged += AddGoldText;
    }
    private void OnDisable()
    {
        currencySystem.CurrencyChanged -= CurrencySystem_CurrencyChanged;
        currencySystem.CurrencyChanged -= AddGold;
        currencySystem.CurrencyChanged -= AddGoldText;
    }
    private void CurrencySystem_CurrencyChanged(int arg1, int arg2)
    {
        text.text = $"Gold : {arg1}";
    }

    //DOTween Ãß°¡
    public void AddGold(int arg1, int arg2)
    {
        text.transform.DOKill();

        text.transform.localScale = originScale;

        text.transform.DOScale(originScale * 1.2f, 0.12f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                text.transform.DOScale(originScale, 0.12f).SetEase(Ease.OutQuad);
            });
    }

    public void AddGoldText(int targetCoin, int arg2)
    {
        int currentGold = currencySystem.Money;
        int tempGold = currentGold;
        DOTween.To(() => tempGold, x =>
        {
            tempGold = x;
            text.text = tempGold.ToString();
        }, targetCoin, 1.0f).OnComplete(() =>
        {
            currentGold = targetCoin;
        });
    }
}
