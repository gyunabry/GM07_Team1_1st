using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SalesCounterDetailView : BuildingDetailView
{
    [Header("판매대 상태")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text capacityText;

    [Header("재고 목록")]
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private SalesInventoryItemView itemViewPrefab;

    [SerializeField] private int slotCount = 12;

    private readonly List<SalesInventoryItemView> itemViews = new();

    private SalesCounter currentCounter;
    private ItemInventory currentInventory;

    private void Awake()
    {
        PrepareSlots();
    }

    private void OnDisable()
    {
        Unbind();
    }

    private void OnDestroy()
    {
        Unbind();
    }

    public override void Bind(IBuildingUIModel building)
    {
        SalesCounter salesCounter = GetBuildingComponent<SalesCounter>(building);

        Unbind();

        if (salesCounter == null) return;

        currentCounter = salesCounter;
        currentCounter.StateChanged += HandleCounterStateChanged;

        SetInventory(currentCounter.Inventory);
        RefreshAll();
    }

    public override bool Supports(IBuildingUIModel building)
    {
        return GetBuildingComponent<SalesCounter>(building) != null;
    }

    public override void Unbind()
    {
        if (currentCounter != null)
        {
            currentCounter.StateChanged -= HandleCounterStateChanged;
        }

        SetInventory(null);
        currentCounter = null;

        if (statusText != null)
        {
            statusText.text = string.Empty;
        }

        if (capacityText != null)
        {
            capacityText.text = "0 / 0";
        }
    }

    private void SetInventory(ItemInventory inventory)
    {
        if (currentInventory == inventory) return;

        if (currentInventory != null)
        {
            currentInventory.InventoryChanged -= HandleInventoryChanged;
        }

        currentInventory = inventory;

        if (currentInventory != null)
        {
            currentInventory.InventoryChanged += HandleInventoryChanged;
        }
    }

    private void RefreshAll()
    {
        RefreshStatus();
        RefreshInventory();
    }

    private void RefreshStatus()
    {
        if (statusText == null) return;

        if (currentCounter == null || !currentCounter.CanOperate)
        {
            statusText.text = "준비 중";
            return;
        }

        statusText.text = currentCounter.IsOpen ? "판매 중" : "판매 중지";
    }

    private void RefreshInventory()
    {
        PrepareSlots();

        for (int i = 0; i < itemViews.Count; i++)
        {
            itemViews[i].SetEmpty();
        }

        if (currentInventory == null)
        {
            capacityText.text = "0 / 0";
            RebuildGridLayout();
            return;
        }

        capacityText.text = $"{currentInventory.TotalAmount} / {currentInventory.Capacity}";

        int slotIndex = 0;

        foreach (InventoryEntry entry in currentInventory.Entries)
        {
            if (entry == null || entry.IsEmpty || entry.Item == null)
            {
                continue;
            }

            if (slotIndex >= itemViews.Count)
            {
                Debug.LogWarning("슬롯이 부족합니다.");
                break;
            }

            // 유효한 아이템을 슬롯 하나에 연결
            itemViews[slotIndex].Bind(entry);
            slotIndex++;
        }
    }

    private void PrepareSlots()
    {
        if (contentRoot == null || itemViewPrefab == null)
        {
            return;
        }

        for (int i = itemViews.Count; i < slotCount; i++)
        {
            SalesInventoryItemView slot = Instantiate(itemViewPrefab, contentRoot);

            slot.name = $"InventorySlot_{i:00}";
            slot.SetEmpty();
            slot.gameObject.SetActive(true);
            itemViews.Add(slot);
        }
    }

    private void RebuildGridLayout()
    {
        if (contentRoot == null) return;

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
    }

    private void HandleInventoryChanged()
    {
        RefreshInventory();
    }

    private void HandleCounterStateChanged()
    {
        SetInventory(currentCounter?.Inventory);
        RefreshAll();
    }
}
