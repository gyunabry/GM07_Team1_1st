using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemHud : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory; 
    [SerializeField] private GameObject itemHud;
    [SerializeField] private List<ItemDataSO> itemUiList;
    [SerializeField] private List<GameObject> uiList;

    private bool isItem; //유무 확인

    private void OnEnable()
    {
        playerInventory.Inventory.InventoryChanged += Inventory_InventoryChanged;
        itemHud.SetActive(false);
    }
    private void OnDisable()
    {
        playerInventory.Inventory.InventoryChanged -= Inventory_InventoryChanged;
    }

    private void Inventory_InventoryChanged()
    {
        isItem = false;

        for (int i = 0; i < uiList.Count; i++)
        {
            if (uiList[i] == null || itemUiList[i] == null)
            {
                itemHud.gameObject.SetActive(false);
                return;
            }

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
                isItem = true;
            }

            if (isItem)
            {
                itemHud.SetActive(true);
            }
            else
            {
                itemHud.SetActive(false);
            }
        }
    }

    //DOTween 추가
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
