using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Tycoon/Building Database")]
public class BuildingDatabaseSO : ScriptableObject
{
    [field: SerializeField]
    public List<BuildingDataSO> BuildingDatas { get; private set; } = new();
    
    // 저장된 ID에 해당하는 시설 데이터를 탐색
    public bool TryGetById(string buildingId, out BuildingDataSO result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(buildingId))
        {
            return false;
        }

        foreach (BuildingDataSO data in BuildingDatas)
        {
            if (data == null) continue;

            if (string.Equals(data.BuildingId, buildingId, System.StringComparison.Ordinal))
            {
                result = data;
                return true;
            }
        }

        return false;
    }
}
