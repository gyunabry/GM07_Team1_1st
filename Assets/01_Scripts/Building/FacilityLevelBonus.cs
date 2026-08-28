using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

    private void OnEnable()
    {
        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.LevelUp += HandleLevelUp;
        }
    }

    private void OnDisable()
    {
        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.LevelUp -= HandleLevelUp;
        }
    }

    private void Start()
    {
        ApplyCurrentLevelLimits();
    }

    private void HandleLevelUp()
    {
        ApplyCurrentLevelLimits();
    }

    private void ApplyCurrentLevelLimits()
    {
        if (buildingDatabase == null) return;

        LevelFacilityLimit currentLimits = FindLimits(CurrencySystem.Instance.Level);

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
