using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Tycoon/Building Database")]
public class BuildingDatabaseSO : ScriptableObject
{
    [field: SerializeField]
    public List<BuildingDataSO> BuildingDatas { get; private set; } = new();
}
