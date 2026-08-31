using System;
using UnityEngine;
using UnityEngine.UI;

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

public enum BuildingTag
{
    None = 0,
    Transmitter,
    Sales,
    Employee,
    Production
}

public enum PlacementLimitScope
{
    Global,
    PerBuildableArea
}


[CreateAssetMenu(fileName = "BuildingDataSO", menuName = "Tycoon/Building Data")]
public class BuildingDataSO : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string buildingId;
    [SerializeField] private string buildingName;
    [SerializeField] private Sprite buildingIcon;
    [SerializeField] private BuildingTag buildingTag;
    [SerializeField, TextArea] private string discription;

    [Header("구매 조건")]
    [SerializeField] private int requiredLevel;
    [SerializeField] private int placementLimit = 3;
    [SerializeField] private int buildCost;

    [SerializeField] private PlacementAreaMask allowedAreas;
    [SerializeField] private SellableType sellable;
    [SerializeField] private float buildTime;

    [SerializeField] private Vector2Int size;
    [SerializeField] private ProcessType supportedProcessType;
    [SerializeField] private GameObject buildingPrefab;
    [SerializeField] private GameObject buildingPreview;

    [SerializeField] private PlacementLimitScope placementLimitScope = PlacementLimitScope.Global;


    public string BuildingName => buildingName;
    public string BuildingId => buildingId;
    public Sprite BuildingIcon => buildingIcon;
    public BuildingTag BuildingTag => buildingTag;
    public string Description => discription;

    public int RequiredLevel => requiredLevel;
    public int PlacementLimit => placementLimit;

    public PlacementAreaMask AllowedAreas => allowedAreas;
    public SellableType Sellable => sellable;
    public int BuildCost => buildCost;

    public float BuildTime => buildTime;

    public Vector2Int Size => size;

    public ProcessType SupportedProcessType => supportedProcessType;
    public GameObject BuildingPrefab => buildingPrefab;
    public GameObject PreviewPrefab => buildingPreview;

    public PlacementLimitScope PlacementLimitScope => placementLimitScope;
}
