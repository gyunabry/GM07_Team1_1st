using System.Collections.Generic;
using UnityEngine;

/*
 [임시]
- 세이브 파일 로드 시 id와 해당 데이터를 비교
 */

public class BuildingCatalog : MonoBehaviour
{
    [SerializeField] private BuildingDatabaseSO database;

    // ID, BuildingData
    private readonly Dictionary<string, BuildingDataSO> dataById = new();

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        foreach (BuildingDataSO data in database.BuildingDatas)
        {
            if (data == null) continue;

            if (!dataById.TryAdd(data.BuildingId, data))
            {
                Debug.LogError($"ID 중복. ID: {data.BuildingId}, 데이터 : {data.name}");
            }
        }
    }

    public bool TryGetBuilding(string id, out BuildingDataSO data)
    {
        return dataById.TryGetValue(id, out data);
    }
}
