using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Tycoon/Building Data")]
public class BuildingDatabaseSO : ScriptableObject
{
    public List<BuildingData> buildingDatas;
}

[System.Serializable]
public class BuildingData
{
    [field:SerializeField]
    public string BuildingName { get; private set; }
    [field: SerializeField]
    public int ID { get; private set; }

    [field: SerializeField]
    public GameObject BuildingPrefab { get; private set; }
    [field: SerializeField]
    public GameObject PreviewPrefab { get; private set; }

    [field: SerializeField]
    public Vector2Int Size { get; private set; } = Vector2Int.one;

    [field: SerializeField]
    public int BuildCost { get; private set; } = 100;
    [field: SerializeField]
    public float BuildTime { get; private set; } = 5f;
}
