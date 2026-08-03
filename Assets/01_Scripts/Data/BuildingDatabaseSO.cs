using System.Collections.Generic;
using UnityEngine;

/*
 에셋 목록 및 검증 담당
 */

[CreateAssetMenu(fileName = "BuildingData", menuName = "Tycoon/Building Database")]
public class BuildingDatabaseSO : ScriptableObject
{
    [field: SerializeField]
    public List<BuildingDataSO> BuildingDatas { get; private set; } = new();
}
