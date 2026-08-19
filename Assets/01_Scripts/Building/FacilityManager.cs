using System;
using System.Collections.Generic;
using UnityEngine;

/*
현재 배치된 시설들을 관리하고 최대 배치 수량을 제한하는 클래스
추후 직원 관리 UI에서 배치된 시설 정보를 확인할 수 있도록 할 예정
 */

public class FacilityManager : MonoBehaviour
{
    public static FacilityManager Instance { get; private set; }

    [SerializeField] private PlacementSystem placementSystem;

    // 시설별 개수를 저장할 딕셔너리
    private readonly Dictionary<BuildingDataSO, int> placedCounts = new();

    public event Action<BuildingDataSO, int> FacilityCountChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (placementSystem == null)
        {
            placementSystem = FindAnyObjectByType<PlacementSystem>();
        }
    }

    private void OnEnable()
    {
        if (placementSystem != null)
        {
            placementSystem.OnBuildingPlaced += HandleBuildingPlaced;
        }
    }

    private void OnDisable()
    {
        if (placementSystem != null)
        {
            placementSystem.OnBuildingPlaced -= HandleBuildingPlaced;
        }
    }

    public int GetPlacedCount(BuildingDataSO data)
    {
        if (data == null) return 0;

        return placedCounts.TryGetValue(data, out int count) ? count : 0;
    }

    public int GetRemainingCount(BuildingDataSO data)
    {
        if (data == null) return 0;

        // 데이터 상에서 최대 설치 가능한 시설 개수
        return Mathf.Max(0, 3);
    }

    public bool CanPlace(BuildingDataSO data)
    {
        // 설치 가능 수보다 적을 때만 배치 가능
        return data != null && GetPlacedCount(data) < 3;
    }

    private void HandleBuildingPlaced(PlacedBuilding building, BuildingDataSO data)
    {
        if (building == null || data == null) return;

        int newCount = GetPlacedCount(data) + 1;
        placedCounts[data] = newCount;

        FacilityCountChanged?.Invoke(data, newCount);
    }

    private void HandleBuildingDestroyed(PlacedBuilding building, BuildingDataSO data)
    {
        if (building == null || data == null) return;

        int newCount = GetPlacedCount(data) + 1;
        placedCounts[data] = newCount;

        FacilityCountChanged?.Invoke(data, newCount);
    }
}
