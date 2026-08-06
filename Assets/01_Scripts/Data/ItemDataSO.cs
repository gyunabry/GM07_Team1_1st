using UnityEngine;

/*
EnemySO
ㄴ DropEntry
    ㄴ ItemDataSO
    ㄴ 드랍 확률 및 수량

RecipeSO
ㄴ 입력 ItemDataSO 리스트
ㄴ 생산 방식
ㄴ 출력 ItemDataSO
*/

public enum ItemType
{
    Material,   // 몬스터 드랍 재료
    Product     // 생산 결과물
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Tycoon/Item Data")]
public class ItemDataSO : ScriptableObject
{
    [field: SerializeField]
    public string ItemId { get; private set; }

    [field: SerializeField]
    public string ItemName { get; private set; }

    [field: SerializeField]
    public string Description { get; private set; }

    [field: SerializeField]
    public ItemType ItemType { get; private set; }

    [field: SerializeField]
    public int SellPrice { get; private set; }

    [field: SerializeField]
    public int Exp { get; private set; }

    [field: SerializeField]
    public int MaxStack { get; private set; }

    [field: SerializeField]
    public Sprite Icon { get; private set; }
}
