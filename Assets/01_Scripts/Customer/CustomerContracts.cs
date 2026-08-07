using System;
using System.Collections.Generic;

// 향후 인벤토리 시스템이 구현할 손님 결제용 최소 계약
public interface ICustomerInventory
{
    event Action InventoryChanged;

    // 모든 재료를 보유한 경우에만 한 번에 차감한다. 부분 차감은 허용하지 않는다.
    bool TryConsumeAll(IReadOnlyList<CustomerOrderItem> items);
}

// 향후 화폐 시스템이 구현할 손님 보상 지급 계약
public interface ICustomerCurrency
{
    void GrantReward(int moneyAmount, int experienceAmount);
}

// 계산대에서 손님 주문을 처리할 수 있는 플레이어 또는 자동 판매직원 표식
public interface IShopCheckoutOperator
{
}

// 주문에 필요한 재료 하나와 수량
[Serializable]
public struct CustomerOrderItem
{
    public string ItemId;
    public int Amount;

    public bool IsValid => !string.IsNullOrWhiteSpace(ItemId) && Amount > 0;
}

// 손님의 복수 재료 주문과 보상 정보
[Serializable]
public struct CustomerOrder
{
    public List<CustomerOrderItem> Items;
    public int Reward;
    public int ExperienceReward;

    public bool IsValid
    {
        get
        {
            if (Items == null || Items.Count == 0 || Reward < 0 || ExperienceReward < 0)
            {
                return false;
            }

            HashSet<string> itemIds = new HashSet<string>();
            for (int i = 0; i < Items.Count; i++)
            {
                CustomerOrderItem item = Items[i];
                if (!item.IsValid || !itemIds.Add(item.ItemId))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

public interface ICustomerState
{
    string Name { get; }
    void Enter();
    void Update();
    void Exit();
}
