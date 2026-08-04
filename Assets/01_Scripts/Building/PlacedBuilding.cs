using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacedBuilding : MonoBehaviour
{
    public BuildingDataSO Data { get; private set; }
    public Vector3Int OriginCell { get; private set; }
    public int RotationIndex { get; private set; }

    // 해당 빌딩이 차지하고 있는 셀
    private readonly List<Vector3Int> occupiedCells = new();

    /// <summary>
    /// 건물 배치 시 초기화하는 메서드.
    /// 배치된 셀, 점유하는 셀, 회전은 배치된 건물 인스턴스가 관리
    /// </summary>
    /// <param name="data">건물 데이터</param>
    /// <param name="originCell">배치 셀 위치</param>
    /// <param name="rotationIndex">회전 인덱스 0 ~ 3</param>
    /// <param name="cells">점유하는 셀</param>
    public void Initialize(BuildingDataSO data, Vector3Int originCell, int rotationIndex, IEnumerable<Vector3Int> cells) 
    {
        Data = data;
        OriginCell = originCell;
        RotationIndex = rotationIndex;

        occupiedCells.Clear();
        occupiedCells.AddRange(cells);
    }
}
