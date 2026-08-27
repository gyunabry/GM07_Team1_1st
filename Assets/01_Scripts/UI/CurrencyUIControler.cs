using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyUIControler : MonoBehaviour
{
    [SerializeField] private TMP_Text currencyText;
    [SerializeField] private Image currencyIcon;

    private Vector3 originTextScale;
    private Vector3 originIconScale;

    private void Awake()
    {
        originTextScale = currencyText.rectTransform.localScale;
        originIconScale = currencyIcon.rectTransform.localScale;
    }

    private void Start()
    {
        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.CurrencyChanged += HandleCurrencyChanged;
            CurrencySystem.Instance.OnGoldChanged += GoldEffect;
            CurrencySystem.Instance.OnGoldEarned += GoldEarnEffect;
            CurrencySystem.Instance.OnGoldSpent += GoldSpendEffect;
            RefreshCoin(CurrencySystem.Instance.Money);
        }
    }

    private void OnDestroy()
    {
        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.CurrencyChanged -= HandleCurrencyChanged;
            CurrencySystem.Instance.OnGoldChanged -= GoldEffect;
            CurrencySystem.Instance.OnGoldEarned -= GoldEarnEffect;
            CurrencySystem.Instance.OnGoldSpent -= GoldSpendEffect;
        }
    }

    private void HandleCurrencyChanged(int money, int exp)
    {
        RefreshCoin(money);
    }

    private void RefreshCoin(int money)
    {
        if (currencyText != null)
        {
            currencyText.text = $"{money}G";
        }
    }

    //DOTween Ãß°¡
    private void GoldEffect()
    {
        currencyText.transform.DOKill();
        currencyIcon.transform.DOKill();

        currencyText.transform.DOScale(1.2f, 0.15f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                currencyText.transform.DOScale(originTextScale, 0.12f).SetEase(Ease.OutQuad);
            });

        currencyIcon.transform.DOScale(1.2f, 0.15f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                currencyIcon.transform.DOScale(originIconScale, 0.12f).SetEase(Ease.OutQuad);
            });
    }

    private void GoldEarnEffect()
    {
        currencyText.DOColor(Color.greenYellow, 0.1f).OnComplete(() => currencyText.DOColor(Color.white, 0.1f));
    }

    private void GoldSpendEffect()
    {
        currencyText.DOColor(Color.orangeRed, 0.1f).OnComplete(() => currencyText.DOColor(Color.white, 0.1f));
    }

}
