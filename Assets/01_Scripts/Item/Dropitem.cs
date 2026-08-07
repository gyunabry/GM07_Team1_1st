using System.Collections;
using UnityEngine;

public class Dropitem : MonoBehaviour
{
    public ItemDataSO dropItem;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private MonsterPoolManager poolManager;
    [SerializeField] private PlayerInventory playerInventory;

    
    public void GetItem()
    {
        Debug.Log("æ∆¿Ã≈€ »πµÊ");
        playerInventory.GiveItem(dropItem, 1);
        poolManager.ReturnPool(this);
    }
    public void GetDropItemData(ItemDataSO dropItem)
    {
        this.dropItem = dropItem;
    }

}
