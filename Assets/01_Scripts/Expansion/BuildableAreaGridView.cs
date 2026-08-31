using System.Collections.Generic;
using UnityEngine;

public class BuildableAreaGridView : MonoBehaviour
{
    [SerializeField] private BuildableArea buildableArea;
    [SerializeField] private Transform gridView;
    [SerializeField] private Renderer gridRenderer;

    [Header("그리드 설정")]
    [SerializeField] private float sourceMeshSize = 10f;
    [SerializeField] private float height = 0.05f;

    private void Reset()
    {
        gridView = transform;
        gridRenderer = GetComponent<Renderer>();
        buildableArea = GetComponentInParent<BuildableArea>();
    }

    private void OnEnable()
    {
        if (buildableArea != null)
        {
            buildableArea.UnlockedAreaChanged += RefreshGrid;
        }
    }

    private void OnDisable()
    {
        if (buildableArea != null)
        {
            buildableArea.UnlockedAreaChanged -= RefreshGrid;
        }
    }

    private void RefreshGrid()
    {
        if (buildableArea == null || buildableArea.Grid == null || gridView == null)
        {
            return;
        }

        if (!TryGetUnlockedBounds(buildableArea.UnlockedAreas, out RectInt bounds))
        {
            return;
        }

        ApplyBounds(buildableArea.Grid, bounds);
    }

    private void ApplyBounds(Grid grid, RectInt bounds)
    {
        Vector3 worldMin = grid.CellToWorld(new Vector3Int(bounds.xMin, bounds.yMin, 0));

        Vector3 worldMax = grid.CellToWorld(new Vector3Int(bounds.xMax, bounds.yMax, 0));

        Transform parent = gridView.parent;

        Vector3 localMin = parent != null
            ? parent.InverseTransformPoint(worldMin) : worldMin;

        Vector3 localMax = parent != null 
            ? parent.InverseTransformPoint(worldMax) : worldMax;

        Vector3 localCenter = (localMin + localMax) * 0.5f;
        localCenter.y = height;

        gridView.localPosition = localCenter;

        float width = Mathf.Abs(localMax.x - localMin.x);
        float depth = Mathf.Abs(localMax.z - localMin.z);

        Vector3 scale = gridView.localScale;
        scale.x = width / sourceMeshSize;
        scale.z = depth / sourceMeshSize;

        gridView.localScale = scale;
    }

    private static bool TryGetUnlockedBounds(IReadOnlyList<RectInt> areas, out RectInt result)
    {
        result = default;

        if (areas == null || areas.Count == 0)
        {
            return false;
        }

        bool found = false;

        int xMin = int.MaxValue;
        int yMin = int.MaxValue;
        int xMax = int.MinValue;
        int yMax = int.MinValue;

        for (int i = 0; i < areas.Count; i++)
        {
            RectInt area = areas[i];

            if (area.width <= 0 || area.height <= 0) continue;

            xMin = Mathf.Min(xMin, area.xMin);
            yMin = Mathf.Min(yMin, area.yMin);
            xMax = Mathf.Max(xMax, area.xMax);
            yMax = Mathf.Max(yMax, area.yMax);

            found = true;
        }

        if (!found) return false;

        result = new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);

        return true;
    }
}
