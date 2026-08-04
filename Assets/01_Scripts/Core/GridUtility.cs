using UnityEngine;

// 마우스 위치에 따른 오브젝트 위치를 일정 셀 단위로 보정하기 위한 전역 클래스
public static class GridUtility
{
    public static Vector3 snapToGrid(Vector3 worldPos, float cellSize)
    {
        float x = Mathf.Round(worldPos.x / cellSize) * cellSize;
        float z = Mathf.Round(worldPos.y / cellSize) * cellSize;

        return new Vector3(x, worldPos.y, z);
    }
}
