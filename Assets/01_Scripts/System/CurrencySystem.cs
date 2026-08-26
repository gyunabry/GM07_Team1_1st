using System;
using System.Collections.Generic;
using UnityEngine;

// TODO: 嫄대Ъ 嫄댁꽕쨌?낃렇?덉씠?쒖? ?щ즺 援щℓ ??TrySpendMoney(鍮꾩슜)瑜??몄텧?섎룄濡??곌껐?쒕떎.
// TODO: HUD媛 CurrencyChanged瑜?援щ룆???덇낵 寃쏀뿕移??쒖떆瑜?媛깆떊?쒕떎.
// TODO: ??Β룸텋?ъ삤湲?湲곕뒫???앷린硫?money? experience瑜??????곸뿉 ?ы븿?쒕떎.

// ?뚮젅?댁뼱媛 媛吏??덇낵 寃쏀뿕移섎? ?쒓납?먯꽌 愿由ы븳??
public sealed class CurrencySystem : MonoBehaviour, ICustomerCurrency
{
    public static CurrencySystem Instance { get; private set; }

    [SerializeField, Min(0)] private int money;
    [SerializeField, Min(0)] private int experience;
    [SerializeField, Min(0)] private int nowExperience;

    // 媛믪씠 諛붾뚮㈃ HUD? ????쒖뒪?쒖씠 ???대깽?몃? 援щ룆??媛깆떊?쒕떎.
    public event Action<int, int> CurrencyChanged;
    public event Action LevelUp;

    public int Money => money;
    public int Experience => experience;


    List<int> needExp = new List<int>() { 60, 110, 260, 330, 430, 800, 950, 1100, 6500, 7200, 10000, 12000, 18000, 20000, 24000, 27000,
    32000, 35000, 40000, 44000};
    private int level = 1;
    public int Level => level;

    public int CurrentExperience => nowExperience;
    public bool IsMaxLevel => level > needExp.Count;

    public int RequiredExpNextLevel => IsMaxLevel ? 0 : needExp[level - 1];

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GrantReward(int moneyAmount, int experienceAmount)
    {
        // ?먮ℓ 蹂댁긽? 媛먯냼?쒗궎???⑸룄媛 ?꾨땲誘濡??뚯닔 媛믪쓣 留됰뒗??
        if (moneyAmount < 0 || experienceAmount < 0)
        {
            Debug.LogWarning("?ы솕 蹂댁긽?먮뒗 ?뚯닔瑜?吏湲됲븷 ???놁뒿?덈떎.", this);
            return;
        }

        // ?ㅼ젣 蹂댁쑀 媛믪쓣 媛깆떊?섎뒗 怨녹쓣 ?섎굹濡?紐⑥븘,
        // 蹂댁긽 異쒖쿂媛 ?섏뼱?섎룄 ?ы솕 蹂寃?洹쒖튃??遺꾩궛?섏? ?딄쾶 ?쒕떎.
        money += moneyAmount;
        experience += experienceAmount;
        nowExperience += experienceAmount;

        // UI? ???湲곕뒫? ???뚮┝留?諛쏆븘??媛곸옄 ?꾩슂??泥섎━瑜??쒕떎.
        CurrencyChanged?.Invoke(money, experience);
    }

    // ?덈쭔 蹂댁긽?쇰줈 以??뚮룄 ?ы솕 蹂寃?泥섎━? ?대깽???몄텧? GrantReward??留↔릿??
    public void GrantMoney(int amount)
    {
        GrantReward(amount, 0);
    }

    // 寃쏀뿕移섎쭔 蹂댁긽?쇰줈 以????ъ슜?쒕떎. ?덈꺼??洹쒖튃? 蹂꾨룄 ?쒖뒪?쒖뿉??泥섎━?쒕떎.
    public void GrantExperience(int amount)
    {
        GrantReward(0, amount);
    }

    // 嫄대Ъ 嫄댁꽕, ?낃렇?덉씠?? ?щ즺 援щℓ泥섎읆 ?덉쓣 吏遺덊빐???섎뒗 紐⑤뱺 湲곕뒫???ъ슜?쒕떎.
    // ?덉씠 遺議깊븯硫?媛믪쓣 諛붽씀吏 ?딄퀬 false瑜?諛섑솚?섎?濡? ?몄텧??履쎌? 援щℓ瑜?吏꾪뻾?섏? ?딅뒗??
    public bool TrySpendMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("?뚮え???덉? ?뚯닔?????놁뒿?덈떎.", this);
            return false;
        }

        if (money < amount)
        {
            return false;
        }

        money -= amount;

        // 援щℓ???깃났??蹂댁쑀 ?덉씠 諛붾뚯뿀?쇰?濡?HUD? ????쒖뒪?쒖뿉 ?뚮┛??
        CurrencyChanged?.Invoke(money, experience);
        return true;
    }
    public bool TrySpendExp(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("?뚮え??寃쏀뿕移섎뒗 ?뚯닔?????놁뒿?덈떎.", this);
            return false;
        }

        if (nowExperience < amount)
        {
            return false;
        }

        nowExperience -= amount;

        // 援щℓ???깃났??蹂댁쑀 ?덉씠 諛붾뚯뿀?쇰?濡?HUD? ????쒖뒪?쒖뿉 ?뚮┛??
        CurrencyChanged?.Invoke(money, experience);
        return true;
    }
    public void TestButton()
    {
        CurrencyChanged?.Invoke(money, experience);
    }


    private void OnEnable()
    {
        CurrencyChanged += CurrencySystem_CurrencyChanged;
    }
    private void OnDisable()
    {
        CurrencyChanged -= CurrencySystem_CurrencyChanged;
    }

    private void CurrencySystem_CurrencyChanged(int arg1, int arg2)
    {
        if (needExp[level-1] <= nowExperience)
        {
            level++;
            LevelUp?.Invoke();
        }
    }
}
