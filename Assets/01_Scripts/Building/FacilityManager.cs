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
    private readonly Dictionary<BuildingDataSO, int> placementLimits = new();

    public event Action<BuildingDataSO, int> FacilityCountChanged;
    public event Action<BuildingDataSO> FacilityInfoChanged;

    // 생산 시설 두 종류가 수를 공유하므로 별도로 관리
    private int productionPlacedCount;

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
            placementSystem.OnBuildingSold += HandleBuildingSold;
        }
    }

    private void OnDisable()
    {
        if (placementSystem != null)
        {
            placementSystem.OnBuildingPlaced -= HandleBuildingPlaced;
            placementSystem.OnBuildingSold -= HandleBuildingSold;
        }
    }

    public int GetPlacedCount(BuildingDataSO data)
    {
        if (data == null) return 0;

        if (data.BuildingTag == BuildingTag.Production)
        {
            return productionPlacedCount;
        }

        return placedCounts.TryGetValue(data, out int count) ? count : 0;
    }

    public int GetRemainingCount(BuildingDataSO data)
    {
        if (data == null) return 0;

        // 데이터 상에서 최대 설치 가능한 시설 개수
        return Mathf.Max(0, GetPlacementLimit(data) - GetPlacedCount(data));
    }

    private int GetConfiguredPlacementLimit(BuildingDataSO data)
    {
        return placementLimits.TryGetValue(data, out int limit) ? limit : data.PlacementLimit;
    }

    public int GetPlacementLimit(BuildingDataSO data)
    {
        if (data == null) return 0;

        int configuredLimit = GetConfiguredPlacementLimit(data);

        if (data.PlacementLimitScope != PlacementLimitScope.PerBuildableArea)
        {
            return configuredLimit;
        }

        int areaCount = 0;

        foreach (BuildableArea area in placementSystem.BuildableAreas)
        {
            if (area != null && area.IsBuildableAllowed(data))
            {
                areaCount++;
            }
        }

        return configuredLimit * areaCount;
    }

    public int GetAreaPlacementLimit(BuildingDataSO data, BuildableArea area)
    {
        if (data == null || area == null || !area.IsBuildableAllowed(data))
        {
            return 0;
        }

        return GetConfiguredPlacementLimit(data);
    }

    public bool CanPlace(BuildingDataSO data)
    {
        // 설치 가능 수보다 적을 때만 배치 가능
        return data != null && GetPlacedCount(data) < GetPlacementLimit(data);
    }

    public void SetPlacementLimit(BuildingDataSO data, int limit)
    {
        if (data == null) return;

        limit = Mathf.Max(0, limit);

        int preLimit = GetConfiguredPlacementLimit(data);

        if (preLimit == limit) return;

        placementLimits[data] = limit;
        FacilityInfoChanged?.Invoke(data);
    }

    private void HandleBuildingPlaced(PlacedBuilding building, BuildingDataSO data)
    {
        if (building == null || data == null) return;

        if (data.BuildingTag == BuildingTag.Production)
        {
            productionPlacedCount++;
        }
        else
        {
            int newCount = GetPlacedCount(data) + 1;
            placedCounts[data] = newCount;
        }

        FacilityInfoChanged?.Invoke(data);
    }

    private void HandleBuildingSold(PlacedBuilding building, int refund)
    {
        if (building == null || building.Data == null) return;

        BuildingDataSO data = building.Data;

        if (data.BuildingTag == BuildingTag.Production)
        {
            productionPlacedCount = Mathf.Max(0, productionPlacedCount - 1);
        }
        else
        {
            int newCount = Mathf.Max(0, GetPlacedCount(data) - 1);

            // 실제 배치 수 갱신
            placedCounts[data] = newCount;
        }

        FacilityInfoChanged?.Invoke(data);
    }
}
