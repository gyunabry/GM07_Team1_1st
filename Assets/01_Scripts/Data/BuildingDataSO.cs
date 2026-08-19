using UnityEngine;

/// <summary>
/// 설치 가능 위치
/// </summary>
public enum PlacementAreaType
{
    Workshop,       // 공방
    HuntingField    // 사냥터
}

[System.Flags]
public enum PlacementAreaMask
{
    None,
    Workshop = 1 << 0,
    HuntingField = 1 << 1,
    All = Workshop | HuntingField
}

[CreateAssetMenu(fileName = "BuildingDataSO", menuName = "Tycoon/Building Data")]
public class BuildingDataSO : ScriptableObject
{
    [field: SerializeField]
    public string BuildingName { get; private set; }
    [field: SerializeField]
    public string BuildingId { get; private set; } // 저장용 ID
    [field: SerializeField]
    public string Description { get; private set; }
    [field:SerializeField]
    [TextArea]
    public PlacementAreaMask AllowedAreas { get; private set; }

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
