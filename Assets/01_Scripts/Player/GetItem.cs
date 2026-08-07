using UnityEngine;

public class GetItem : MonoBehaviour
{
    private PlayerInventory playerInventory;

    private void Awake()
    {
        playerInventory = GetComponentInParent<PlayerInventory>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out Dropitem drop))
        {
            return;
        }

        int added = playerInventory.Inventory.Add(drop.Item, drop.Amount);

        if (added > 0)
        {
            drop.Collect();
        }
    }
}
