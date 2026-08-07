using System;
using UnityEngine;

// TODO: 건물 건설·업그레이드와 재료 구매 시 TrySpendMoney(비용)를 호출하도록 연결한다.
// TODO: HUD가 CurrencyChanged를 구독해 돈과 경험치 표시를 갱신한다.
// TODO: 저장·불러오기 기능이 생기면 money와 experience를 저장 대상에 포함한다.

// 플레이어가 가진 돈과 경험치를 한곳에서 관리한다.
public sealed class CurrencySystem : MonoBehaviour, ICustomerCurrency
{
    [SerializeField, Min(0)] private int money;
    [SerializeField, Min(0)] private int experience;

    // 값이 바뀌면 HUD와 저장 시스템이 이 이벤트를 구독해 갱신한다.
    public event Action<int, int> CurrencyChanged;

    public int Money => money;
    public int Experience => experience;

    public void GrantReward(int moneyAmount, int experienceAmount)
    {
        // 판매 보상은 감소시키는 용도가 아니므로 음수 값을 막는다.
        if (moneyAmount < 0 || experienceAmount < 0)
        {
            Debug.LogWarning("재화 보상에는 음수를 지급할 수 없습니다.", this);
            return;
        }

        // 실제 보유 값을 갱신하는 곳을 하나로 모아,
        // 보상 출처가 늘어나도 재화 변경 규칙이 분산되지 않게 한다.
        money += moneyAmount;
        experience += experienceAmount;

        // UI와 저장 기능은 이 알림만 받아서 각자 필요한 처리를 한다.
        CurrencyChanged?.Invoke(money, experience);
    }

    // 돈만 보상으로 줄 때도 재화 변경 처리와 이벤트 호출은 GrantReward에 맡긴다.
    public void GrantMoney(int amount)
    {
        GrantReward(amount, 0);
    }

    // 경험치만 보상으로 줄 때 사용한다. 레벨업 규칙은 별도 시스템에서 처리한다.
    public void GrantExperience(int amount)
    {
        GrantReward(0, amount);
    }

    // 건물 건설, 업그레이드, 재료 구매처럼 돈을 지불해야 하는 모든 기능이 사용한다.
    // 돈이 부족하면 값을 바꾸지 않고 false를 반환하므로, 호출한 쪽은 구매를 진행하지 않는다.
    public bool TrySpendMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("소모할 돈은 음수일 수 없습니다.", this);
            return false;
        }

        if (money < amount)
        {
            return false;
        }

        money -= amount;

        // 구매에 성공해 보유 돈이 바뀌었으므로 HUD와 저장 시스템에 알린다.
        CurrencyChanged?.Invoke(money, experience);
        return true;
    }
}
