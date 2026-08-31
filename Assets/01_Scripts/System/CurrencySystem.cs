using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class CurrencySystem : MonoBehaviour, ICustomerCurrency
{
    public static CurrencySystem Instance { get; private set; }

    [SerializeField, Min(0)] private int money;
    [SerializeField, Min(0)] private int experience;
    [SerializeField, Min(0)] private int nowExperience;

    public event Action<int, int> CurrencyChanged;
    public event Action LevelUp;

    // DOTween ÀÌº¥Æ®
    public event Action OnGoldChanged;
    public event Action OnGoldEarned;
    public event Action OnGoldSpent;

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
        if (moneyAmount < 0 || experienceAmount < 0)
        {
            Debug.LogWarning("?¬í™” ë³´ìƒ?ëŠ” ?Œìˆ˜ë¥?ì§€ê¸‰í•  ???†ìŠµ?ˆë‹¤.", this);
            return;
        }

        money += moneyAmount;
        experience += experienceAmount;
        nowExperience += experienceAmount;

        CurrencyChanged?.Invoke(money, experience);
        OnGoldChanged?.Invoke();
        OnGoldEarned?.Invoke();
    }

    public void GrantMoney(int amount)
    {
        GrantReward(amount, 0);
    }

    public void GrantExperience(int amount)
    {
        GrantReward(0, amount);
        CheckLevelUp();
    }

    public bool TrySpendMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("?Œëª¨???ˆì? ?Œìˆ˜?????†ìŠµ?ˆë‹¤.", this);
            return false;
        }

        if (money < amount)
        {
            return false;
        }

        money -= amount;

        CurrencyChanged?.Invoke(money, experience);
        OnGoldChanged?.Invoke();
        OnGoldSpent?.Invoke();
        return true;
    }
    public bool TrySpendExp(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("?Œëª¨??ê²½í—˜ì¹˜ëŠ” ?Œìˆ˜?????†ìŠµ?ˆë‹¤.", this);
            return false;
        }

        if (nowExperience < amount)
        {
            return false;
        }

        nowExperience -= amount;

        CurrencyChanged?.Invoke(money, experience);
        return true;
    }

    public void TestButton()
    {
        CurrencyChanged?.Invoke(money, experience);
        OnGoldChanged?.Invoke();
    }

    private void CheckLevelUp()
    {
        if (needExp[level - 1] <= nowExperience)
        {
            level++;
            LevelUp?.Invoke();
        }
    }

    public void RestoreState(
        int savedMoney, 
        int savedTotalExperience, 
        int savedCurrentExperience, 
        int savedLevel)
    {
        money = Mathf.Max(0, savedMoney);
        experience = Mathf.Max(0, savedTotalExperience);
        nowExperience = Mathf.Max(0, savedCurrentExperience);

        // ÃÖ´ë ·¹º§Àº °æÇèÄ¡ Å×ÀÌºí °³¼ö + 1 (ÀÎµ¦½º)
        level = Mathf.Clamp(savedLevel, 1, needExp.Count + 1);

        CurrencyChanged?.Invoke(money, experience);
        OnGoldChanged?.Invoke();
    }
}
