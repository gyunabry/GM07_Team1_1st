using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemHud : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory; 
    [SerializeField] private List<ItemDataSO> itemUiList;
    [SerializeField] private List<GameObject> uiList;


    private void OnEnable()
    {
        playerInventory.Inventory.InventoryChanged += Inventory_InventoryChanged;
    }
    private void OnDisable()
    {
        playerInventory.Inventory.InventoryChanged -= Inventory_InventoryChanged;
    }

    private void Inventory_InventoryChanged()
    {
        for(int i = 0; i < uiList.Count; i++)
        {
            if (uiList[i] == null || itemUiList[i] == null) return;
            Image selectUi = uiList[i].GetComponentInChildren<Image>();
            selectUi.sprite = itemUiList[i].Icon;
            TextMeshProUGUI selectText = uiList[i].GetComponentInChildren<TextMeshProUGUI>();
            selectText.text = $"{playerInventory.GetAmount(itemUiList[i])}";
            if (playerInventory.Inventory.GetAmount(itemUiList[i]) <= 0)
            {
                uiList[i].SetActive(false);
            }
            else
            {
                uiList[i].SetActive(true);
                ItemEffect(uiList[i]);
            }
        }
    }

    //DOTween Ãß°¡
    private void ItemEffect(GameObject ui)
    {
        ui.transform.DOKill();

        ui.transform.DOScale(1.2f, 0.15f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                ui.transform.DOScale(Vector3.one, 0.12f).SetEase(Ease.OutQuad);
            });
    }
}
