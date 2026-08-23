using System;
using UnityEngine;

// TODO: ê±´ë¬¼ ê±´ì„¤Â·?…ê·¸?ˆì´?œì? ?¬ë£Œ êµ¬ë§¤ ??TrySpendMoney(ë¹„ìš©)ë¥??¸ì¶œ?˜ë„ë¡??°ê²°?œë‹¤.
// TODO: HUDê°€ CurrencyChangedë¥?êµ¬ë…???ˆê³¼ ê²½í—˜ì¹??œì‹œë¥?ê°±ì‹ ?œë‹¤.
// TODO: ?€?¥Â·ë¶ˆ?¬ì˜¤ê¸?ê¸°ëŠ¥???ê¸°ë©?money?€ experienceë¥??€???€?ì— ?¬í•¨?œë‹¤.

// ?Œë ˆ?´ì–´ê°€ ê°€ì§??ˆê³¼ ê²½í—˜ì¹˜ë? ?œê³³?ì„œ ê´€ë¦¬í•œ??
public sealed class CurrencySystem : MonoBehaviour, ICustomerCurrency
{
    [SerializeField, Min(0)] private int money;
    [SerializeField, Min(0)] private int experience;

    // ê°’ì´ ë°”ë€Œë©´ HUD?€ ?€???œìŠ¤?œì´ ???´ë²¤?¸ë? êµ¬ë…??ê°±ì‹ ?œë‹¤.
    public event Action<int, int> CurrencyChanged;

    public int Money => money;
    public int Experience => experience;

    public void GrantReward(int moneyAmount, int experienceAmount)
    {
        // ?ë§¤ ë³´ìƒ?€ ê°ì†Œ?œí‚¤???©ë„ê°€ ?„ë‹ˆë¯€ë¡??Œìˆ˜ ê°’ì„ ë§‰ëŠ”??
        if (moneyAmount < 0 || experienceAmount < 0)
        {
            Debug.LogWarning("?¬í™” ë³´ìƒ?ëŠ” ?Œìˆ˜ë¥?ì§€ê¸‰í•  ???†ìŠµ?ˆë‹¤.", this);
            return;
        }

        // ?¤ì œ ë³´ìœ  ê°’ì„ ê°±ì‹ ?˜ëŠ” ê³³ì„ ?˜ë‚˜ë¡?ëª¨ì•„,
        // ë³´ìƒ ì¶œì²˜ê°€ ?˜ì–´?˜ë„ ?¬í™” ë³€ê²?ê·œì¹™??ë¶„ì‚°?˜ì? ?Šê²Œ ?œë‹¤.
        money += moneyAmount;
        experience += experienceAmount;

        // UI?€ ?€??ê¸°ëŠ¥?€ ???Œë¦¼ë§?ë°›ì•„??ê°ì ?„ìš”??ì²˜ë¦¬ë¥??œë‹¤.
        CurrencyChanged?.Invoke(money, experience);
    }

    // ?ˆë§Œ ë³´ìƒ?¼ë¡œ ì¤??Œë„ ?¬í™” ë³€ê²?ì²˜ë¦¬?€ ?´ë²¤???¸ì¶œ?€ GrantReward??ë§¡ê¸´??
    public void GrantMoney(int amount)
    {
        GrantReward(amount, 0);
    }

    // ê²½í—˜ì¹˜ë§Œ ë³´ìƒ?¼ë¡œ ì¤????¬ìš©?œë‹¤. ?ˆë²¨??ê·œì¹™?€ ë³„ë„ ?œìŠ¤?œì—??ì²˜ë¦¬?œë‹¤.
    public void GrantExperience(int amount)
    {
        GrantReward(0, amount);
    }

    // ê±´ë¬¼ ê±´ì„¤, ?…ê·¸?ˆì´?? ?¬ë£Œ êµ¬ë§¤ì²˜ëŸ¼ ?ˆì„ ì§€ë¶ˆí•´???˜ëŠ” ëª¨ë“  ê¸°ëŠ¥???¬ìš©?œë‹¤.
    // ?ˆì´ ë¶€ì¡±í•˜ë©?ê°’ì„ ë°”ê¾¸ì§€ ?Šê³  falseë¥?ë°˜í™˜?˜ë?ë¡? ?¸ì¶œ??ìª½ì? êµ¬ë§¤ë¥?ì§„í–‰?˜ì? ?ŠëŠ”??
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

        // êµ¬ë§¤???±ê³µ??ë³´ìœ  ?ˆì´ ë°”ë€Œì—ˆ?¼ë?ë¡?HUD?€ ?€???œìŠ¤?œì— ?Œë¦°??
        CurrencyChanged?.Invoke(money, experience);
        return true;
    }
    public bool TrySpendExp(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("?Œëª¨??ê²½í—˜ì¹˜ëŠ” ?Œìˆ˜?????†ìŠµ?ˆë‹¤.", this);
            return false;
        }

        if (experience < amount)
        {
            return false;
        }

        experience -= amount;

        // êµ¬ë§¤???±ê³µ??ë³´ìœ  ?ˆì´ ë°”ë€Œì—ˆ?¼ë?ë¡?HUD?€ ?€???œìŠ¤?œì— ?Œë¦°??
        CurrencyChanged?.Invoke(money, experience);
        return true;
    }
    public void TestButton()
    {
        CurrencyChanged?.Invoke(money, experience);
    }
}
