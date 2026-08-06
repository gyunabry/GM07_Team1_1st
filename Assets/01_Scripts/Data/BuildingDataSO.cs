using UnityEngine;

//public enum BuildingType
//{
    
//}

[CreateAssetMenu(fileName = "BuildingDataSO", menuName = "Tycoon/Building Data")]
public class BuildingDataSO : ScriptableObject
{
    [field: SerializeField]
    public string BuildingName { get; private set; }
    [field: SerializeField]
    public string BuildingId { get; private set; } // ÀúÀå¿ë ID

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
