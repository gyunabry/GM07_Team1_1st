using UnityEngine;

[DisallowMultipleComponent]
public sealed class HuntingTransmitter : MonoBehaviour
{
    [SerializeField] private Transform depositPoint;
    [SerializeField] private ItemInventory inventory = new();
    public Transform DepositPoint => depositPoint != null ? depositPoint : transform;
    public ItemInventory Inventory => inventory;
}
