using UnityEngine;

[CreateAssetMenu(fileName = "DropItem", menuName = "ScriptableObjects/DropItem")]
public class DropItemSO : ScriptableObject
{
    public string itemName;
    public string itemDes;
    public int itemValue;
    public int itemExp;
    public float dropChance;
    public Sprite itemSprite;
}
