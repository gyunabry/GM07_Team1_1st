using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacedBuilding : MonoBehaviour
{
    public BuildingData Data { get; private set; }
    public Vector3Int OriginCell { get; private set; }
    public int RotationIndex { get; private set; }

    // 해당 빌딩이 차지하고 있는 셀
    private readonly List<Vector3Int> occupiedCells = new();

    public void Initialize(BuildingData data, Vector3Int originCell, int rotationIndex, IEnumerable<Vector3Int> cells) 
    {
        Data = data;
        OriginCell = originCell;
        RotationIndex = rotationIndex;

        occupiedCells.Clear();
        occupiedCells.AddRange(cells);
    }
}
