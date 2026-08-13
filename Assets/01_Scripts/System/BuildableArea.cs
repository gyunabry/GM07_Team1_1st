using System.Collections.Generic;
using UnityEngine;

public class BuildableArea : MonoBehaviour
{
    [SerializeField] private Grid grid;

    // 현재 해금되어 시설 배치가 가능한 영역
    [SerializeField] private List<RectInt> unlockedAreas = new();

    public bool IsBuildable(Vector3Int cell)
    {
        Vector2Int floorCell = new Vector2Int(cell.x, cell.y);

        foreach (RectInt area in unlockedAreas)
        {
            if (area.Contains(floorCell))
            {
                return true;
            }
        }

        return false;
    }

    // 공방 확장 시 호출해 배치 가능 영역 확장
    public void UnlockArea(RectInt area)
    {
        unlockedAreas.Add(area);
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
