using System.Collections;
using UnityEngine;

public class Dropitem : MonoBehaviour
{
    public DropItemSO dropItem;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private MonsterPoolManager poolManager;
    private Coroutine co;

    private void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1f, playerLayer);
        if(colliders.Length > 0)
        {
            co = StartCoroutine(ItemGet());
        }
    }
    IEnumerator ItemGet()
    {
        yield return new WaitForSeconds(0.1f);
        Debug.Log("æ∆¿Ã≈€ »πµÊ");
        poolManager.ReturnPool(this);
        co = null;
    }
    public void GetDropItemData(DropItemSO dropItem)
    {
        this.dropItem = dropItem;
    }

}
