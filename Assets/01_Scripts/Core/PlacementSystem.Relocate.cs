using System.Collections.Generic;
using UnityEngine;

// 재배치 시스템

public partial class PlacementSystem : MonoBehaviour
{
    // 기존 위치를 담는 클래스
    private class RelocationSnapshot
    {
        public PlacedBuilding Building;
        public BuildableArea Area;
        public Vector3Int OriginCell;
        public short RotationIndex;
        public List<Vector3Int> OccupiedCells;
    }

    private RelocationSnapshot relocationSnapshot;

    public void BeginRelocateMode()
    {
        if (CurrentMode != PlacementMode.None) return;

        ClearSelection();
        ChangeMode(PlacementMode.RelocateSelect);
    }

    // 재배치 시도
    public bool TryBeginRelocate(PlacedBuilding building)
    {
        if (building == null || building.Data == null || building.AssignedArea == null)
        {
            return false;
        }

        if (CurrentMode != PlacementMode.None &&
            CurrentMode != PlacementMode.RelocateSelect)
        {
            return false;
        }

        ClearSelection();

        selectedPlacedBuilding = building;
        selectedBuildingData = building.Data;
        currentArea = building.AssignedArea;
        rotationIndex = building.RotationIndex;

        relocationSnapshot = new RelocationSnapshot
        {
            Building = building,
            Area = currentArea,
            OriginCell = building.OriginCell,
            RotationIndex = building.RotationIndex,
            OccupiedCells = new List<Vector3Int>(building.OccupiedCells)
        };

        CreatePreview();

        if (previewObject == null)
        {
            ClearRelocationRuntime();

            return false;
        }

        previewObject.transform.SetPositionAndRotation(
            building.transform.position,
            building.transform.rotation
        );

        //building.SelectionVisual?.SetState(
        //    BuildingSelectionState.Selected
        //);

        SelectionChanged?.Invoke(building);
        ChangeMode(PlacementMode.RelocatePlacement);

        return true;
    }

    // 이미 배치된 시설을 눌러 재배치 모드에 진입하는 메서드
    private void TrySelectRelocateTarget()
    {
        if (!inputManager.TryGetBuilding(out PlacedBuilding building))
        {
            return;
        }

        TryBeginRelocate(building);
    }

    // 새로운 위치에 배치하는 메서드
    private void TryConfirmRelocation()
    {
        if (CurrentMode != PlacementMode.RelocatePlacement ||
            relocationSnapshot == null ||
            selectedPlacedBuilding == null ||
            currentArea == null ||
            !canPlace)
        {
            return;
        }

        PlacedBuilding building = selectedPlacedBuilding;

        BuildableArea oldArea = relocationSnapshot.Area;
        IReadOnlyList<Vector3Int> oldCells = relocationSnapshot.OccupiedCells;

        Vector2Int rotatedSize = GetRotatedSize(selectedBuildingData.Size, rotationIndex);
        List<Vector3Int> newCells = GetOccupiedCells(currentCell, rotatedSize);

        if (!currentArea.AreCellsAvailable(newCells, building))
        {
            return;
        }

        Vector3 worldPosition = GetBuildingCenter(currentArea.Grid, currentCell, rotatedSize);

        Quaternion worldRotation = GetRotation(rotationIndex);

        oldArea.Release(building, oldCells);

        if (!currentArea.TryOccupy(building, newCells))
        {
            // 실패 시 기존 점유 복구
            oldArea.TryOccupy(building, oldCells);

            return;
        }

        building.ApplyPlacement(
            currentArea,
            currentCell,
            rotationIndex,
            newCells,
            worldPosition,
            worldRotation
        );

        ClearRelocationRuntime();
        ChangeMode(PlacementMode.RelocateSelect);

        OnBuildingMoved?.Invoke(building);
    }

    // 재배치 취소
    private void CancelRelocation()
    {
        ClearRelocationRuntime();
    }

    //
    private void ClearRelocationRuntime()
    {
        //if (selectedPlacedBuilding != null)
        //{
        //    selectedPlacedBuilding.SelectionVisual?.SetState(BuildingSelectionState.None);
        //}

        if (previewObject != null) Destroy(previewObject);

        previewObject = null;
        relocationSnapshot = null;
        selectedPlacedBuilding = null;
        selectedBuildingData = null;
        currentArea = null;
        rotationIndex = 0;
        canPlace = false;

        SelectionChanged?.Invoke(null);
    }

    public void ToggleRelocateMode()
    {
        // RelocateSelect / RelocatePlacement라면 None
        if (IsRelocateMode)
        {
            ExitCurrentMode();
            return;
        }

        // 기존 모드 취소
        if (CurrentMode != PlacementMode.None)
        {
            ExitCurrentMode();
        }

        BeginRelocateMode();
    }
}
