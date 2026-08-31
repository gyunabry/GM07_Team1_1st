using TMPro;
using UnityEngine;

public class SkillTreeCoinHud : MonoBehaviour
{
    [SerializeField] CurrencySystem currencySystem;
    TextMeshProUGUI text;

    private void Awake()
    {
        if(currencySystem == null)
        {
            currencySystem = FindAnyObjectByType<CurrencySystem>();
        }
        text = GetComponentInChildren<TextMeshProUGUI>();
    }
    private void OnEnable()
    {
        text.text = currencySystem.Money.ToString();
        currencySystem.CurrencyChanged += CurrencySystem_CurrencyChanged;
    }
    private void OnDisable()
    {
        currencySystem.CurrencyChanged -= CurrencySystem_CurrencyChanged;
    }
    private void CurrencySystem_CurrencyChanged(int arg1, int arg2)
    {
        text.text = arg1.ToString();
    }
}
