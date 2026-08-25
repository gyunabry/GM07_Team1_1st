using System.Collections.Generic;
using UnityEngine;

public class BuildableArea : MonoBehaviour
{
    [SerializeField] private string areaId;
    [SerializeField] private AreaType areaType;
    [SerializeField] private Grid grid;
    [SerializeField] private Collider placementSurface;
    [SerializeField] private Transform buildingContainer;
    [SerializeField] private GameObject gridView;

    // 현재 해금되어 시설 배치가 가능한 영역
    [SerializeField] private List<RectInt> unlockedAreas = new();
    [SerializeField] private List<RectInt> blockedAreas = new();

    private readonly Dictionary<Vector3Int, PlacedBuilding> occupiedCells = new();

    public string AreaId => areaId;
    public AreaType AreaType => areaType;
    public Grid Grid => grid;
    public Collider PlacementSurface => placementSurface;
    public Transform BuildingContainer => buildingContainer;

    // 현재 영역 타입을 BuildingDataSO의 마스크와 비교할 때 사용
    public PlacementAreaMask AreaMask
    {
        get
        {
            return areaType switch
            {
                AreaType.Workshop => PlacementAreaMask.Workshop,

                AreaType.HuntingField => PlacementAreaMask.HuntingField,

                _ => PlacementAreaMask.None
            };
        }
    }

    // 해당 시설이 해당 영역에 설치 가능한지 검사
    public bool IsBuildableAllowed(BuildingDataSO buildingData)
    {
        if (buildingData == null) return false;

        // 해당 시설 데이터의 마스크와 설치 가능 마스크가 동일한지 판별
        return (buildingData.AllowedAreas & AreaMask) != 0;
    }

    // 월드 좌표를 이 영역의 셀 좌표로 변환
    public bool TryWorldToCell(Vector3 worldPosition, out Vector3Int cell)
    {
        cell = default;

        if (grid == null) return false;

        cell = grid.WorldToCell(worldPosition);

        cell.z = 0;

        return true;
    }

    // 해당 셀이 설치 가능한 위치인지 반환
    public bool IsBuildable(Vector3Int cell)
    {
        cell.z = 0;

        bool isUnlocked = IsCellInsideAnyArea(cell, unlockedAreas);

        if (!isUnlocked) return false;

        bool isBlocked = IsCellInsideAnyArea(cell, blockedAreas);

        return !isBlocked;
    }

    public bool IsBlueprintBuildable(Vector3Int originCell, Vector2Int size)
    {
        if (size.x <= 0 || size.y <= 0) return false;

        originCell.z = 0;

        // 사이즈 영역만큼 배치가 가능한지 검사
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int cell = originCell + new Vector3Int(x, y, 0);

                if (!IsBuildable(cell)) return false;
            }
        }

        return true;
    }

    public bool CanPlaceBuilding(BuildingDataSO buildingData, Vector3Int originCell, Vector2Int size)
    {
        return IsBuildableAllowed(buildingData) && IsBlueprintBuildable(originCell, size);
    }

    public Vector3 GetCellCenterWorld(Vector3Int cell)
    {
        if (grid == null) return transform.position;

        cell.z = 0;
        return grid.GetCellCenterWorld(cell);
    }

    // 공방 확장 시 호출해 배치 가능 영역 확장
    public void UnlockArea(RectInt area)
    {
        unlockedAreas.Add(area);
    }

    public void SetGridVisible(bool visible)
    {
        if (gridView != null)
        {
            gridView.SetActive(visible);
        }
    }

    private static bool IsCellInsideAnyArea(Vector3Int cell, List<RectInt> areas)
    {
        Vector2Int floorCell = new(cell.x, cell.y);

        foreach (RectInt area in areas)
        {
            if (area.Contains(floorCell))
            {
                return true;
            }
        }

        return false;
    }

    public bool AreCellsAvailable(IReadOnlyList<Vector3Int> cells, PlacedBuilding ignoredBuilding = null)
    {
        if (cells == null || cells.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < cells.Count; i++)
        {
            Vector3Int cell = NormalizeCell(cells[i]);

            if (!occupiedCells.TryGetValue(cell, out PlacedBuilding owner))
            {
                continue;
            }

            // 재배치 중인 자기 자신은 허용
            if (owner != ignoredBuilding)
            {
                return false;
            }
        }

        return true;
    }

    // 점유 시도
    public bool TryOccupy(PlacedBuilding building, IReadOnlyList<Vector3Int> cells)
    {
        if (building == null || !AreCellsAvailable(cells))
        {
            return false;
        }

        for (int i = 0; i < cells.Count; i++)
        {
            occupiedCells[NormalizeCell(cells[i])] = building;
        }

        return true;
    }

    // 점유 해제
    public void Release(PlacedBuilding building, IReadOnlyList<Vector3Int> cells)
    {
        if (building == null || cells == null)
        {
            return;
        }

        for (int i = 0; i < cells.Count; i++)
        {
            Vector3Int cell = NormalizeCell(cells[i]);

            // 선택된 시설이 실제 점유하고 있는 시설과 같다면 딕셔너리에서 제거
            if (occupiedCells.TryGetValue(cell, out PlacedBuilding owner) && owner == building)
            {
                occupiedCells.Remove(cell);
            }
        }
    }

    public bool IsOccupied(Vector3Int cell)
    {
        return occupiedCells.ContainsKey(NormalizeCell(cell));
    }

    private static Vector3Int NormalizeCell(Vector3Int cell)
    {
        cell.z = 0;
        return cell;
    }

    private void OnDrawGizmosSelected()
    {
        if (grid == null) return;

        Gizmos.color = Color.cyan;

        foreach (RectInt area in unlockedAreas)
        {
            for (int x = area.xMin; x < area.xMax; x++)
            {
                for (int depth = area.yMin;
                     depth < area.yMax;
                     depth++)
                {
                    Vector3Int cell =
                        new Vector3Int(x, depth, 0);

                    Vector3 center = grid.GetCellCenterWorld(cell);

                    center.y = 0.02f;

                    Gizmos.DrawWireCube(
                        center,
                        new Vector3(
                            grid.cellSize.x,
                            0.02f,
                            grid.cellSize.y
                        )
                    );
                }
            }
        }
    }
}
