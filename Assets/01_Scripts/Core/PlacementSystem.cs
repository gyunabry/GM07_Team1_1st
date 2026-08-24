using System;
using System.Collections.Generic;
using UnityEngine;

/*
 상태 패턴 활용 리팩토링 예정
- Placement
- Remove
- Interior
 */

public class PlacementSystem : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Grid grid;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private InputManager inputManager;
    // [SerializeField] private BuildableArea buildableArea;
    [SerializeField] private List<BuildableArea> buildableAreas = new();
    [SerializeField] private Transform buildingContainer;
    [SerializeField] private EconomyModifierService economyModifier;

    private BuildableArea currentArea;

    // 셀별로 어떤 건물이 점유 중인지 저장
    private readonly Dictionary<Vector3Int, PlacedBuilding> occupiedCells = new();

    // 현재 배치 대상으로 선택된 건물 데이터
    private BuildingDataSO selectedBuildingData;
    private GameObject previewObject;
    private Vector3Int currentCell;

    // 0 : 0도 / 1: 90도 / 2: 180도 / 3: 270도
    private short rotationIndex;
    private bool canPlace;

    public bool IsPlacementMode => selectedBuildingData != null;
    public BuildingDataSO SelectedBuildingData => selectedBuildingData;

    public event Action<PlacedBuilding, BuildingDataSO> OnBuildingPlaced;

    private void OnEnable()
    {
        if (inputManager == null) return;

        // 이벤트로 입력 처리
        inputManager.OnClicked += PlaceBuilding;
        inputManager.OnExit += CancelPlacement;
        inputManager.OnRotation += RotatePreview;
    }

    private void OnDisable()
    {
        if (inputManager == null) return;

        inputManager.OnClicked -= PlaceBuilding;
        inputManager.OnExit -= CancelPlacement;
        inputManager.OnRotation -= RotatePreview;
    }

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        // 건설 모드가 아니라면 즉시 반환
        if (!IsPlacementMode) return;

        UpdatePreview();
    }

    // BuildingDatabaseSO의 배열 인덱스로 건물 선택 후 프리뷰 생성
    // 버튼에 직접 연결해 사용
    public void StartPlacement(BuildingDataSO buildingData)
    {
        if (buildingData == null)
        {
            Debug.LogWarning("배치할 건물 데이터가 지정되지 않았습니다.");
            return;
        }

        // 기존 선택된 건물 배치를 취소하기 위해 Cancel 호출
        CancelPlacement();

        selectedBuildingData = buildingData;
        rotationIndex = 0;

        CreatePreview();
    }
    
    // 현재 배치 모드를 종료
    public void CancelPlacement()
    {
        selectedBuildingData = null;
        currentArea = null;
        rotationIndex = 0;
        canPlace = false;

        if (previewObject != null)
        {
            Destroy(previewObject);
        }

        previewObject = null;
    }

    // BuildingData의 프리뷰 프리팹을 생성
    public void CreatePreview()
    {
        previewObject = Instantiate(selectedBuildingData.PreviewPrefab);
        // 해당 프리뷰 오브젝트에 콜라이더가 있다면 비활성화
        if (previewObject.TryGetComponent<Collider>(out Collider collider))
        {
            collider.enabled = false;
        }

        previewObject.name = $"{selectedBuildingData.BuildingName}_Preview";
    }

    // 현재 마우스 위치를 기준으로 프리뷰 위치와 배치 가능 여부 갱신
    private void UpdatePreview()
    {
        if (previewObject == null || inputManager == null || grid == null) return;

        if (inputManager.IsPointerOverUI())
        {
            HidePreview();
            return;
        }

        if (!inputManager.TryGetPlacementHit(out Vector3 worldPos, out Collider hitCollider))
        {
            HidePreview();
            return;
        }

        currentArea = FindBuildableArea(hitCollider);

        if (currentArea == null)
        {
            HidePreview();
            return;
        }

        previewObject.SetActive(true);

        // 해당 월드 위치가 어떤 셀에 있는지 저장
        currentCell = grid.WorldToCell(worldPos);
        currentCell.z = 0;

        Vector2Int rotatedSize = GetRotatedSize(selectedBuildingData.Size, rotationIndex);

        Vector3 previewPos = GetBuildingCenter(currentCell, rotatedSize);
        previewPos.y = 0;

        previewObject.transform.SetPositionAndRotation(previewPos, GetRotation(rotationIndex));

        canPlace = IsCellsAvailable(currentArea, currentCell, rotatedSize);

        UpdatePreviewVisual(canPlace);
    }

    // R 키를 눌렀을 때 프리뷰 시계방향 회전
    private void RotatePreview()
    {
        if (!IsPlacementMode || previewObject == null) return;

        // 회전 인덱스 증가
        rotationIndex = (short)((rotationIndex + 1) % 4);

        // 프리뷰 회전
        previewObject.transform.rotation = GetRotation(rotationIndex);

        UpdatePreview();
    }

    // 프리뷰 위치에 실제 건물 인스턴스 생성
    private void PlaceBuilding()
    {
        if (currentArea == null || !canPlace || previewObject == null)
            return;

        int finalCost = economyModifier.GetBuildCost(selectedBuildingData);
        if (!CurrencySystem.Instance.TrySpendMoney(finalCost))
        {
            return;
        }

        Vector2Int rotatedSize = GetRotatedSize(selectedBuildingData.Size, rotationIndex);

        List<Vector3Int> cells = GetOccupiedCells(currentCell, rotatedSize);

        Vector3 buildingPos = GetBuildingCenter(currentCell, rotatedSize);
        buildingPos.y = 0;

        GameObject buildingObj = Instantiate(
            selectedBuildingData.BuildingPrefab,
            buildingPos,
            GetRotation(rotationIndex),
            buildingContainer
        );

        buildingObj.name = selectedBuildingData.BuildingName;

        PlacedBuilding placedBuilding = buildingObj.GetComponent<PlacedBuilding>();
        if (placedBuilding != null)
        {
            placedBuilding.Initialize(
                selectedBuildingData, 
                currentArea,
                currentCell, 
                rotationIndex, 
                cells
            );

            // 설치된 건물이 차지하는 모든 셀을 등록
            foreach (Vector3Int cell in cells)
            {
                occupiedCells.Add(cell, placedBuilding);
            }
        }

        placedBuilding.BeginConstruction();

        OnBuildingPlaced?.Invoke(placedBuilding, selectedBuildingData);

        // 건물을 반복적으로 설치할 수 있도록 프리뷰 갱신 (직전 설치 위치에는 설치 X 표시)
        UpdatePreview();
    }

    // 건물이 차지할 모든 셀이 비어있는지 검사
    private bool IsCellsAvailable(BuildableArea area, Vector3Int originCell, Vector2Int size)
    {
        if (area == null) return false;

        if (!area.CanPlaceBuilding(selectedBuildingData, originCell, size))
        {
            return false;
        }

        if (IsHunterBuilding(selectedBuildingData))
        {
            HuntingFieldContext fieldContext = area.GetComponent<HuntingFieldContext>();
            if (fieldContext == null || !fieldContext.TryGetCompletedTransmitter(out _))
            {
                return false;
            }
        }

        foreach (Vector3Int cell in GetOccupiedCells(originCell, size))
        {
            if (occupiedCells.ContainsKey(cell))
            {
                return false;
            }
        }

        //for (int x = 0; x < size.x; x++)
        //{
        //    for (int z = 0; z < size.y; z++)
        //    {
        //        Vector3Int cell = originCell + new Vector3Int(x, z, 0);

        //        // 공방 밖이면 배치 불가
        //        if (!buildableArea.IsBuildable(cell))
        //        {
        //            return false;
        //        }

        //        // 해당 셀이 이미 등록되어 있다면 false 반환
        //        if (occupiedCells.ContainsKey(cell))
        //        {
        //            return false;
        //        }
        //    }
        //}

        return true;
    }

    // 건물이 차지할 셀 목록을 생성
    // 설치된 건물이 자신의 점유 셀을 저장하거나 삭제할 때 사용
    private List<Vector3Int> GetOccupiedCells(Vector3Int originCell, Vector2Int size)
    {
        List<Vector3Int> cells = new(size.x * size.y);

        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                cells.Add(originCell + new Vector3Int(x, z, 0));
            }
        }

        return cells;
    }

    // 건물이 차지하는 전체 셀 영역의 중앙 위치를 반환
    private Vector3 GetBuildingCenter(Vector3Int originCell, Vector2Int size)
    {
        Vector3Int lastCell = originCell + new Vector3Int(size.x - 1, size.y - 1, 0);

        Vector3 firstCenter = grid.GetCellCenterWorld(originCell);
        Vector3 lastCenter = grid.GetCellCenterWorld(lastCell);

        Vector3 center = (firstCenter + lastCenter) * 0.5f;
        center.y = 0f;

        return center;
    }

    // 건물에 회전을 적용했을 때 차지하는 크기
    private Vector2Int GetRotatedSize(Vector2Int originSize, int targetRotationIndex)
    {
        bool swapXToY = targetRotationIndex == 1 || targetRotationIndex == 3;

        return swapXToY ? new Vector2Int(originSize.y, originSize.x) : originSize;
    }

    // 회전 인덱스를 Y축 회전값으로 변환
    private Quaternion GetRotation(short targetRotationIndex)
    {
        return Quaternion.Euler(
            0f,
            targetRotationIndex * 90f,
            0f
        );
    }

    private void UpdatePreviewVisual(bool isValid)
    {
        if (!previewObject.TryGetComponent(out BuildingPreview buildingPreview))
        {
            return;
        }

        buildingPreview.SetPreview(isValid);
    }

    // 해당 셀이 이미 점유된 상태인지 반환
    public bool IsCellOccupied(Vector3Int cell)
    {
        cell.z = 0;
        return occupiedCells.ContainsKey(cell);
    }

    private BuildableArea FindBuildableArea(Collider hitColldier)
    {
        foreach (BuildableArea area in buildableAreas)
        {
            if (area == null) continue;

            if (area.PlacementSurface == hitColldier)
            {
                return area;
            }
        }

        return null;
    }

    private void HidePreview()
    {
        currentArea = null;
        canPlace = false;

        if (previewObject != null)
        {
            previewObject.gameObject.SetActive(false);
        }
    }

    // 임시 메서드
    private static bool IsHunterBuilding(BuildingDataSO buildingData)
    {
        if (buildingData == null || buildingData.BuildingPrefab == null) 
        {
            return false;
        }

        return buildingData.BuildingPrefab.TryGetComponent<HunterBuildingController>(out _);
    }
}
