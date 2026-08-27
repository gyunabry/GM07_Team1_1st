using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUIController : MonoBehaviour
{
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Slider expSlider;
    [SerializeField] private TMP_Text expText;

    private CurrencySystem currencySystem;

    private void Start()
    {
        currencySystem = CurrencySystem.Instance;

        currencySystem.CurrencyChanged += HandleCurrencyChanged;

        RefreshUI();
    }

    private void OnDestroy()
    {
        if (currencySystem != null)
        {
            currencySystem.CurrencyChanged -= HandleCurrencyChanged;
        }
    }

    private void HandleCurrencyChanged(int money, int exp)
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        levelText.text = $"{currencySystem.Level}";
        expSlider.wholeNumbers = true;

        if (currencySystem.IsMaxLevel)
        {
            expSlider.minValue = 0;
            expSlider.maxValue = 1;
            expSlider.value = 1;

            expText.text = "Max";
            return;
        }

        // 레벨 경험치 구간
        int currentExp = currencySystem.CurrentExperience;
        int requiredExp = currencySystem.RequiredExpNextLevel;

        currentExp = Mathf.Clamp(currentExp, 0, requiredExp);

        expSlider.minValue = 0;
        expSlider.maxValue = requiredExp;
        expSlider.value = currentExp;

        expText.text = $"{currentExp} / {requiredExp}";
    }
}
