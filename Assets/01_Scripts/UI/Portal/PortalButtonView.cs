using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PortalButtonView : MonoBehaviour
{
    [SerializeField] private HuntingFieldUnlockDataSO fieldData;
    [SerializeField] private Button button;
    [SerializeField] private GameObject lockRoot;
    [SerializeField] private TMP_Text requiredLevelText;
    [SerializeField] private TMP_Text costText;

    public HuntingFieldUnlockDataSO Data => fieldData;
    public Button Button => button;

    public void Refresh(bool portalExists, bool unlocked, bool canUnlock)
    {
        bool levelSatisfied = CurrencySystem.Instance != null && CurrencySystem.Instance.Level >= fieldData.RequiredLevel;
        bool moneySatisfied = CurrencySystem.Instance != null && CurrencySystem.Instance.Money >= fieldData.UnlockCost;

        if (button != null)
        {
            // 연결된 포탈이 있고, 해금이 가능하거나 되어 있는 경우에만 상호작용 가능
            button.interactable = portalExists && (unlocked || canUnlock);
        }

        if (lockRoot != null)
        {
            lockRoot.SetActive(!unlocked);
        }
        
        if (requiredLevelText != null)
        {
            requiredLevelText.text = $"Lv. {Data.RequiredLevel}";
            requiredLevelText.color = levelSatisfied ? Color.green : Color.red;
        }

        if (costText != null)
        {
            costText.text = $"{Data.UnlockCost:N0}G";
            costText.color = moneySatisfied ? Color.green : Color.red;
        }
    }
}
