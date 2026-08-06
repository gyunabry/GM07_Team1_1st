using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Tycoon/Item Database")]
public class ItemDatabaseSO : ScriptableObject
{
    [field: SerializeField]
    public List<ItemDataSO> Items = new List<ItemDataSO>();
}
