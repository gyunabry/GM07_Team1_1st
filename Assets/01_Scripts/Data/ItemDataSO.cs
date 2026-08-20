using UnityEngine;

public enum ItemType
{
    Material,   // 몬스터 드랍 재료
    Product     // 생산 결과물
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Tycoon/Item Data")]
public class ItemDataSO : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string itemName;
    [SerializeField] private string description;

    [SerializeField] private ItemType itemType;
    [SerializeField] private ProcessType processType;

    [SerializeField] private int sellPrice;
    [SerializeField] private int exp;

    [SerializeField] private Sprite icon;

    public string ItemId => itemId;
    public string ItemName => itemName;
    public string Description => description;

    public ItemType ItemType => itemType;
    public ProcessType ProcessType => processType;

    public int SellPrice => sellPrice;
    public int Exp => exp;

    public Sprite Icon => icon;
}
