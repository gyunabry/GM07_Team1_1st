using System;
using System.Collections.Generic;
using UnityEngine;

// 사냥터 해금 상태를 관리하는 매니저 클래스

public class HuntingFieldManager : MonoBehaviour
{
    [SerializeField] private List<HuntingFieldUnlockDataSO> fields = new();

    // 해금된 사냥터 ID를 저장하는 해시셋
    private readonly HashSet<string> unlockedIds = new();

    public event Action StateChanged;

    // 해당 필드가 해금된 상태인지 검사
    public bool IsUnlocked(HuntingFieldUnlockDataSO data)
    {
        return data != null && (data.IsUnlocked || unlockedIds.Contains(data.DestinationId));
    }

    // 해당 필드를 해금할 수 있는 상태인지 검사
    public bool CanUnlock(HuntingFieldUnlockDataSO data)
    {
        CurrencySystem currency = CurrencySystem.Instance;

        return data != null &&
            currency != null &&
            !IsUnlocked(data) &&
            currency.Level >= data.RequiredLevel &&
            currency.Money >= data.UnlockCost;
    }

    // 해당 사냥터를 해금 시도
    public bool TryUnlock(HuntingFieldUnlockDataSO data)
    {
        if (!CanUnlock(data))
        {
            return false;
        }

        if (!CurrencySystem.Instance.TrySpendMoney(data.UnlockCost))
        {
            return false;
        }

        unlockedIds.Add(data.DestinationId);
        StateChanged?.Invoke();

        return true;
    }

    // 해금된 사냥터 id를 반환
    public List<string> CaptureUnlockedIds()
    {
        return new List<string>(unlockedIds);
    }

    public void RestoreUnlockedIds(IEnumerable<string> savedIds)
    {
        unlockedIds.Clear();

        if (savedIds != null)
        {
            foreach (string id in savedIds)
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    unlockedIds.Add(id);
                }
            }
        }

        StateChanged?.Invoke();
    }
}
