using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUIController : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Slider expSlider;
    [SerializeField] private TMP_Text expText;

    private CurrencySystem currencySystem;

    private void Start()
    {
        currencySystem = CurrencySystem.Instance;

        currencySystem.CurrencyChanged += HandleCurrencyChanged;

        RefreshUI(currencySystem.Experience);
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
        RefreshUI(exp);
    }

    private void RefreshUI(int totalExp)
    {
        levelText.text = $"{player.NowLevel}";
        expSlider.wholeNumbers = true;

        if (player.IsMaxLevel)
        {
            expSlider.minValue = 0;
            expSlider.maxValue = 1;
            expSlider.value = 1;

            expText.text = "Max";
            return;
        }

        // 레벨 경험치 구간
        int levelStartExp = player.CurrentLevelStartExp;
        int nextLevelExp = player.RequiredExpNextLevel;

        int requiredExp = nextLevelExp - levelStartExp;
        int currentExp = totalExp - levelStartExp;

        currentExp = Mathf.Clamp(currentExp, 0, requiredExp);

        expSlider.minValue = 0;
        expSlider.maxValue = requiredExp;
        expSlider.value = currentExp;

        expText.text = $"{currentExp} / {requiredExp}";
    }
}
