using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 직원이 시간 기반 작업을 수행하는 동안 머리 위에 상품 작업 진행도를 표시합니다.
/// </summary>
[DisallowMultipleComponent]
public sealed class EmployeeWorkProgressHUD : MonoBehaviour
{
    private GameObject hudInstance;
    private Slider progressSlider;
    private Image itemIcon;
    private TMP_Text amountText;
    private bool isWorking;
    private bool hasCargo;
    private bool isPositionAdjusted;

    public void ShowProgress(float normalizedProgress)
    {
        ShowProgress(normalizedProgress, null, 0, 0);
    }

    public void ShowProgress(float normalizedProgress, ItemInventory inventory)
    {
        if (inventory != null)
        {
            foreach (InventoryEntry entry in inventory.Entries)
            {
                if (entry == null || entry.IsEmpty)
                {
                    continue;
                }

                ShowProgress(normalizedProgress, entry.Item, inventory.TotalAmount, inventory.Capacity);
                return;
            }
        }

        ShowProgress(normalizedProgress, null, 0, 0);
    }

    public void ShowProgress(float normalizedProgress, ItemDataSO item, int totalAmount, int capacity)
    {
        isWorking = true;
        EnsureHud();
        if (hudInstance == null)
        {
            return;
        }

        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(true);
            progressSlider.SetValueWithoutNotify(Mathf.Clamp01(normalizedProgress));
        }

        UpdateCargo(item, totalAmount, capacity);
    }

    public void Hide()
    {
        isWorking = false;
        UpdateHudVisibility();
    }

    public void RefreshCargo(ItemInventory inventory)
    {
        if (inventory != null)
        {
            foreach (InventoryEntry entry in inventory.Entries)
            {
                if (entry == null || entry.IsEmpty)
                {
                    continue;
                }

                RefreshCargo(entry.Item, inventory.TotalAmount, inventory.Capacity);
                return;
            }
        }

        RefreshCargo(null, 0, 0);
    }

    public void RefreshCargo(ItemDataSO item, int totalAmount, int capacity)
    {
        EnsureHud();
        if (hudInstance != null)
        {
            UpdateCargo(item, totalAmount, capacity);
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

        hudInstance = FindChildHud("UI_ProductHUD");
        if (hudInstance == null)
        {
            return;
        }

        RectTransform rectTransform = hudInstance.GetComponent<RectTransform>();
        if (rectTransform != null && !isPositionAdjusted)
        {
            rectTransform.anchoredPosition += Vector2.up * 0.7f;
            isPositionAdjusted = true;
        }

        progressSlider = hudInstance.GetComponentInChildren<Slider>(true);
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

    private void UpdateCargo(ItemDataSO item, int totalAmount, int capacity)
    {
        hasCargo = item != null && totalAmount > 0;
        if (item != null && totalAmount > 0)
        {
            if (itemIcon != null)
            {
                itemIcon.sprite = item.Icon;
                itemIcon.enabled = itemIcon.sprite != null;
            }

            if (amountText != null)
            {
                amountText.text = $"{totalAmount} / {Mathf.Max(0, capacity)}";
            }

            SetCargoVisible(true);
            UpdateHudVisibility();
            return;
        }

        SetCargoVisible(false);
        UpdateHudVisibility();
    }

    private void SetCargoVisible(bool isVisible)
    {
        if (itemIcon != null)
        {
            itemIcon.gameObject.SetActive(isVisible);
        }

        if (amountText != null)
        {
            amountText.gameObject.SetActive(isVisible);
        }
    }

    private void UpdateHudVisibility()
    {
        if (hudInstance == null)
        {
            return;
        }

        bool shouldShowHud = isWorking || hasCargo;
        if (hudInstance.activeSelf != shouldShowHud)
        {
            hudInstance.SetActive(shouldShowHud);
        }

        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(isWorking);
        }
    }
}
