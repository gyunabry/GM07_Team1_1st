using System;
using System.Collections.Generic;
using UnityEngine;

public class FacilityLevelBonus : MonoBehaviour
{
    [Serializable]
    private class LevelFacilityLimit
    {
        public int level;

        
        public int productionLimit;
        public int salesLimit;
    }

    [SerializeField] private BuildingDatabaseSO buildingDatabase;
    [SerializeField] private List<LevelFacilityLimit> limits = new();

    private void Start()
    {
        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.LevelChanged += HandleLevelChanged;
        }

        ApplyCurrentLevelLimits(CurrencySystem.Instance.Level);
    }

    private void OnDisable()
    {
        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.LevelChanged -= HandleLevelChanged;
        }
    }

    private void HandleLevelChanged(int currentLevel)
    {
        ApplyCurrentLevelLimits(currentLevel);
    }

    private void ApplyCurrentLevelLimits(int level)
    {
        if (buildingDatabase == null) return;

        Debug.Log($"레벨별 시설 제한 복구 완료. 복구 기준 레벨: {level}");

        LevelFacilityLimit currentLimits = FindLimits(level);

        if (currentLimits == null) return;

        foreach (BuildingDataSO buildingData in buildingDatabase.BuildingDatas)
        {
            if (buildingData == null) continue;

            if (!TryGetLimit(buildingData, currentLimits, out int limit))
            {
                continue;
            }

            FacilityManager.Instance.SetPlacementLimit(buildingData, limit);
        }
    }

    private LevelFacilityLimit FindLimits(int level)
    {
        LevelFacilityLimit result = null;

        foreach (LevelFacilityLimit limit in limits)
        {
            if (limit == null) continue;

            if (limit.level <= level && 
                (result == null || limit.level > result.level))
            {
                result = limit;
            }
        }

        return result;
    }

    private bool TryGetLimit(BuildingDataSO buildingData, LevelFacilityLimit limits, out int limit)
    {
        switch (buildingData.BuildingTag)
        {
            case BuildingTag.Production:
                limit = limits.productionLimit;
                return true;

            case BuildingTag.Sales:
                limit = limits.salesLimit;
                return true;

            default:
                limit = 0;
                return false;
        }
    }
}
