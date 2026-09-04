using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUIController : MonoBehaviour
{
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Slider expSlider;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private Image expFillImage;

    private Color originColor;

    private CurrencySystem currencySystem;

    private void Start()
    {
        originColor = expFillImage.color;

        currencySystem = CurrencySystem.Instance;

        currencySystem.CurrencyChanged_EXP += HandleCurrencyChanged;

        currencySystem.LevelChanged += CurrencySystem_LevelChanged;

        RefreshUI();
    }

    private void CurrencySystem_LevelChanged(int currentLevel)
    {
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (currencySystem != null)
        {
            currencySystem.CurrencyChanged_EXP -= HandleCurrencyChanged;
            currencySystem.LevelChanged -= CurrencySystem_LevelChanged;
        }
    }

    private void HandleCurrencyChanged(int exp)
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

        ExpTextEffect();
        ExpSliderEffect();
    }

    //DOTween 추가
    private void ExpTextEffect()
    {
        expText.transform.DOKill();

        expText.transform.DOScale(1.2f, 0.15f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                expText.transform.DOScale(Vector3.one, 0.12f).SetEase(Ease.OutQuad);
            });
    }

    private void ExpSliderEffect()
    {
        expFillImage.DOKill();

        expFillImage.DOColor(Color.softYellow, 0.1f).OnComplete(() => expFillImage.DOColor(originColor, 0.1f));
    }
}
