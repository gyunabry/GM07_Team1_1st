using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 직원이 들고 있는 첫 번째 아이템과 전체 소지량을 머리 위에 표시합니다.
/// </summary>
[DisallowMultipleComponent]
public sealed class EmployeeCargoHUD : MonoBehaviour
{
    private GameObject hudInstance;
    private Image itemIcon;
    private TMP_Text amountText;

    public void Refresh(ItemInventory inventory)
    {
        if (inventory == null || inventory.TotalAmount <= 0)
        {
            Hide();
            return;
        }

        IReadOnlyList<InventoryEntry> entries = inventory.Entries;
        for (int i = 0; i < entries.Count; i++)
        {
            InventoryEntry entry = entries[i];
            if (entry != null && !entry.IsEmpty)
            {
                Show(entry.Item, entry.Amount, inventory.Capacity);
                return;
            }
        }

        Hide();
    }

    public void Show(ItemDataSO item, int totalAmount, int capacity)
    {
        if (item == null || totalAmount <= 0)
        {
            Hide();
            return;
        }

        EnsureHud();
        if (hudInstance == null)
        {
            return;
        }

        itemIcon.sprite = item.Icon;
        itemIcon.enabled = itemIcon.sprite != null;
        amountText.text = $"{totalAmount} / {Mathf.Max(0, capacity)}";
        hudInstance.SetActive(true);
    }

    public void Hide()
    {
        if (hudInstance != null)
        {
            hudInstance.SetActive(false);
        }
    }

    private void OnDisable()
    {
        Hide();
    }

    private void EnsureHud()
    {
        if (hudInstance != null)
        {
            return;
        }

        hudInstance = FindChildHud("UI_CharacterHUD");
        if (hudInstance == null)
        {
            return;
        }

        Transform[] children = hudInstance.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name == "Inven_Icon")
            {
                itemIcon = children[i].GetComponent<Image>();
            }
            else if (children[i].name == "Inven_Text")
            {
                amountText = children[i].GetComponent<TMP_Text>();
            }
        }

        if (itemIcon == null || amountText == null)
        {
            Debug.LogError("UI_CharacterHUD must contain Inven_Icon and Inven_Text.", this);
            Destroy(hudInstance);
            hudInstance = null;
            return;
        }

        hudInstance.SetActive(false);
    }

    private GameObject FindChildHud(string hudName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != transform && children[i].name == hudName)
            {
                return children[i].gameObject;
            }
        }

        return null;
    }
}
