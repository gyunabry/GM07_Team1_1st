using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TransmitterDetailView : BuildingDetailView
{
    [Header("패널")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GraphicRaycaster graphicRaycaster;

    [Header("통합전송기 정보")]
    [SerializeField] private TMP_Text buildingName;
    [SerializeField] private TMP_Text capacityText;

    [Header("재고 목록")]
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private SalesInventoryItemView itemViewPrefab;

    [SerializeField] private int slotCount = 24;

    [SerializeField] private StorageDecomposition storageDecomposition;

    private readonly List<SalesInventoryItemView> itemViews = new();

    private ItemInventory currentInventory;

    private void Awake()
    {
        PrepareSlots();

        SetVisible(false);
    }

    private void OnDisable()
    {
        Close();
    }

    private void OnDestroy()
    {
        Close();
    }

    public override void Open(IBuildingUIModel building)
    {
        IntegratedTransmitter transmitter = GetBuildingComponent<IntegratedTransmitter>(building);

        Close();

        if (transmitter == null) return;

        if (buildingName != null)
        {
            buildingName.text = building.BuildingName;
        }

        SetInventory(transmitter.Inventory);
        RefreshInventory();
        SetVisible(true);
    }

    public override bool Supports(IBuildingUIModel building)
    {
        return GetBuildingComponent<IntegratedTransmitter>(building) != null;
    }

    public override void Close()
    {
        SetVisible(false);

        SetInventory(null);
        ClearSlots();

        if (buildingName != null)
        {
            buildingName.text = string.Empty;
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

    private void RefreshInventory()
    {
        PrepareSlots();
        ClearSlots();

        if (currentInventory == null)
        {
            if (capacityText != null)
            {
                capacityText.text = "0 / 0";
            }
            
            RebuildGridLayout();
            return;
        }

        if (capacityText != null)
        {
            capacityText.text = $"{currentInventory.TotalAmount} / {currentInventory.Capacity}";
        }
 
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
            itemViews[slotIndex].Bind(entry, currentInventory, storageDecomposition);
            slotIndex++;
        }

        RebuildGridLayout();
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

    private void ClearSlots()
    {
        for (int i = 0; i < itemViews.Count; i++)
        {
            itemViews[i].SetEmpty();
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

    private void SetVisible(bool visible)
    {
        if (canvas != null)
        {
            canvas.enabled = visible;
        }

        if (graphicRaycaster != null)
        {
            graphicRaycaster.enabled = visible;
        }
    }
}
