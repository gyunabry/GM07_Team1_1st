using UnityEngine;

/// <summary>
/// 설치 가능 위치
/// </summary>
public enum AreaType
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

public enum SellableType
{
    Impossible = 0,
    Possible,
    Patial
}

[CreateAssetMenu(fileName = "BuildingDataSO", menuName = "Tycoon/Building Data")]
public class BuildingDataSO : ScriptableObject
{
    [SerializeField] private string buildingId;
    [SerializeField] private string buildingName;
    [SerializeField, TextArea] private string discription;
    [SerializeField] private PlacementAreaMask allowedAreas;
    [SerializeField] private SellableType sellable;
    [SerializeField] private int buildCost;
    [SerializeField] private float buildTime;

    [SerializeField] private Vector2Int size;
    [SerializeField] private ProcessType supportedProcessType;
    [SerializeField] private GameObject buildingPrefab;
    [SerializeField] private GameObject buildingPreview;


    public string BuildingName => buildingName;
    public string BuildingId => buildingId;
    public string Description => discription;
    public PlacementAreaMask AllowedAreas => allowedAreas;
    public SellableType Sellable => sellable;
    public int BuildCost => buildCost;

    public float BuildTime => buildTime;

    public Vector2Int Size => size;

    public ProcessType SupportedProcessType => supportedProcessType;
    public GameObject BuildingPrefab => buildingPrefab;
    public GameObject PreviewPrefab => buildingPreview;
}
