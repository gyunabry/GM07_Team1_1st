using TMPro;
using UnityEngine;

public class CurrencyUIControler : MonoBehaviour
{
    [SerializeField] private TMP_Text currencyText;

    private void Start()
    {
        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.CurrencyChanged += HandleCurrencyChanged;
            RefreshCoin(CurrencySystem.Instance.Money);
        }
    }

    private void OnDestroy()
    {
        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.CurrencyChanged -= HandleCurrencyChanged;
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
}
