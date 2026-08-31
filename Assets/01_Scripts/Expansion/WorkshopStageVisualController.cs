using System;
using UnityEngine;

public class WorkshopStageVisualController : MonoBehaviour
{
    [Serializable]
    private class StageVisual 
    {
        [SerializeField] private GameObject floorRoot;
        [SerializeField] private GameObject wallRoot;
        [SerializeField] private GameObject obstacleRoot;

        public GameObject FloorRoot => floorRoot;
        public GameObject WallRoot => wallRoot;
        public GameObject ObstacleRoot => obstacleRoot;

        public void SetActive(bool active)
        {
            if (floorRoot != null) floorRoot.SetActive(active);
            if (wallRoot != null) wallRoot.SetActive(active);
            if (obstacleRoot != null) obstacleRoot.SetActive(!active);
        }
    }

    [SerializeField] private StageVisual[] stages;

    public int CurrentStage { get; private set; }
    public int MaxStage => stages?.Length ?? 0;

    // 현재 스테이지 값을 통해 해당되는 단계의 바닥과 벽 활성화
    public void ApplyStage(int stage)
    {
        CurrentStage = stage;

        for (int i = 0; i < stages.Length; i++)
        {
            stages[i]?.SetActive(i < stage);
        }
    }

    public bool TryGetStageRoots(int stage, out GameObject floorRoot, out GameObject wallRoot, out GameObject obstacleRoot)
    {
        floorRoot = null;
        wallRoot = null;
        obstacleRoot = null;

        int index = stage - 1;

        if (index < 0 || stages == null || index >= stages.Length || stages[index] == null)
        {
            return false;
        }

        floorRoot = stages[index].FloorRoot;
        wallRoot = stages[index].WallRoot;
        obstacleRoot = stages[index].ObstacleRoot;

        return floorRoot != null || wallRoot != null || obstacleRoot != null;
    }
}
