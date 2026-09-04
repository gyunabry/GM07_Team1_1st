using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlacementMode
{
    None,
    PurchasePlacement,  // 구매할 시설 선택 상태
    RelocateSelect,     // 재배치 선택
    RelocatePlacement,  // 재배치 모드 중 시설 선택 상태
    SellSelect,         // 판매 선택
    SellConfirm         // 판매 모드 중 시설 선택 상태
}

public partial class PlacementSystem : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Grid grid;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private List<BuildableArea> buildableAreas = new();
    [SerializeField] private Transform buildingContainer;
    [SerializeField] private EconomyModifierService economyModifier;

    private BuildableArea currentArea;

    // 현재 배치 대상으로 선택된 건물 데이터
    private BuildingDataSO selectedBuildingData;
    private PlacedBuilding selectedPlacedBuilding;
    private GameObject previewObject;
    private Vector3Int currentCell;

    // 0 : 0도 / 1: 90도 / 2: 180도 / 3: 270도
    private short rotationIndex;
    private bool canPlace;

    public IReadOnlyList<BuildableArea> BuildableAreas => buildableAreas;

    public bool IsBuildModeActive { get; private set; }

    [field: SerializeField]
    public PlacementMode CurrentMode { get; private set; }
    public PlacedBuilding SelectedPlacedBuilding => selectedPlacedBuilding;
    
    public bool ConsumeWorldInput => IsBuildModeActive || CurrentMode != PlacementMode.None;

    // 배치 모드 여부
    public bool IsPlacementMode =>
        CurrentMode == PlacementMode.PurchasePlacement ||
        CurrentMode == PlacementMode.RelocatePlacement;

    public bool IsRelocateMode =>
        CurrentMode == PlacementMode.RelocateSelect ||
        CurrentMode == PlacementMode.RelocatePlacement;

    public bool IsSellMode =>
        CurrentMode == PlacementMode.SellSelect ||
        CurrentMode == PlacementMode.SellConfirm;

    public event Action<PlacementMode> ModeChanged;
    public event Action<PlacedBuilding> SelectionChanged;

    public event Action<PlacedBuilding, BuildingDataSO> OnBuildingPlaced;
    public event Action<PlacedBuilding> OnBuildingMoved;
    public event Action<PlacedBuilding, int> OnBuildingSold;

    private void OnEnable()
    {
        if (inputManager == null) return;

        inputManager.OnPrimaryClicked += HandlePrimaryClick;
        inputManager.OnSecondaryClicked += HandleSecondaryClick;
        inputManager.OnBuildingLongPressed += HandleBuildingLongPressed;
    }

    private void OnDisable()
    {
        if (inputManager == null) return;

        inputManager.OnPrimaryClicked -= HandlePrimaryClick;
        inputManager.OnSecondaryClicked -= HandleSecondaryClick;
        inputManager.OnBuildingLongPressed -= HandleBuildingLongPressed;
    }

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Start()
    {
        RegisterPreplacedBuildings();
    }

    private void Update()
    {
        // 건설 모드가 아니라면 즉시 반환
        if (!IsPlacementMode) return;

        UpdatePreview();
    }

    // BuildingDatabaseSO의 배열 인덱스로 건물 선택 후 프리뷰 생성
    // 버튼에 직접 연결해 사용
    public void StartPlacement(BuildingDataSO data)
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_Comfirm);

        StartPurchasePlacement(data);
    }

    public bool StartPurchasePlacement(BuildingDataSO data)
    {
        if (data == null) return false;

        BuildingPurchaseStatus status = EvaluatePurchase(data);
        if (!status.CanPurchase)
        {
            return false;
        }

        // 현재 선택된 시설과 같은 시설 버튼을 클릭했을 때 반환
        if (CurrentMode == PlacementMode.PurchasePlacement &&
            selectedBuildingData == data)
        {
            CancelCurrentAction();
            return true;
        }

        // 기존 상태 종료
        ExitCurrentMode();

        selectedBuildingData = data;
        rotationIndex = 0;

        CreatePreview();

        if (previewObject == null)
        {
            selectedBuildingData = null;
            return false;
        }

        ChangeMode(PlacementMode.PurchasePlacement);

        return true;
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

    public void CancelCurrentAction()
    {
        switch (CurrentMode)
        {
            case PlacementMode.PurchasePlacement:
                ClearPurchaseRuntime();
                ChangeMode(PlacementMode.None);
                break;

            case PlacementMode.RelocateSelect:
                ChangeMode(PlacementMode.None);
                break;

            case PlacementMode.RelocatePlacement:
                ClearRelocationRuntime();
                ChangeMode(PlacementMode.RelocateSelect);
                break;

            case PlacementMode.SellSelect:
                ChangeMode(PlacementMode.None);
                break;

            case PlacementMode.SellConfirm:
                ClearSelection();
                ChangeMode(PlacementMode.SellSelect);
                break;
        }
    }

    public void ExitCurrentMode()
    {
        switch (CurrentMode)
        {
            case PlacementMode.PurchasePlacement:
                ClearPurchaseRuntime();
                break;

            case PlacementMode.RelocatePlacement:
                ClearRelocationRuntime();
                break;

            case PlacementMode.SellConfirm:
                ClearSelection();
                break;
        }

        AudioManager.Instance.PlaySFX(ESFXType.UI_Cancel);
        ChangeMode(PlacementMode.None);
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
        if (previewObject == null || inputManager == null) return;

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

        if (currentArea == null || currentArea.Grid == null)
        {
            HidePreview();
            return;
        }

        previewObject.SetActive(true);

        // 해당 월드 위치가 어떤 셀에 있는지 저장
        currentCell = currentArea.Grid.WorldToCell(worldPos);
        currentCell.z = 0;

        Vector2Int rotatedSize = GetRotatedSize(selectedBuildingData.Size, rotationIndex);

        Vector3 previewPos = GetBuildingCenter(currentArea.Grid, currentCell, rotatedSize);
        previewPos.y = 0;

        previewObject.transform.SetPositionAndRotation(previewPos, GetRotation(rotationIndex));

        bool isPositionValid = IsCellsAvailable(currentArea, currentCell, rotatedSize);
        bool isPurchaseValid = 
            CurrentMode != PlacementMode.PurchasePlacement || 
            EvaluatePurchase(selectedBuildingData).CanPurchase;

        canPlace = isPositionValid && isPurchaseValid;

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

        Vector3 buildingPos = GetBuildingCenter(currentArea.Grid, currentCell, rotatedSize);
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

        }

        placedBuilding.BeginConstruction();

        OnBuildingPlaced?.Invoke(placedBuilding, selectedBuildingData);

        // 건물을 반복적으로 설치할 수 있도록 프리뷰 갱신 (직전 설치 위치에는 설치 X 표시)
        UpdatePreview();
    }

    #region 유틸
    // 건물이 차지할 모든 셀이 비어있는지 검사
    private bool IsCellsAvailable(BuildableArea area, Vector3Int originCell, Vector2Int size)
    {
        if (area == null || selectedBuildingData == null) return false;

        if (selectedBuildingData.PlacementLimitScope == PlacementLimitScope.PerBuildableArea)
        {
            int currentCount = 
                area.GetPlacedCount(selectedBuildingData, selectedPlacedBuilding);

            int areaLimit = 
                FacilityManager.Instance.GetAreaPlacementLimit(selectedBuildingData, area);

            if (currentCount >= areaLimit)
            {
                return false;
            }
        }

        if (!area.CanPlaceBuilding(selectedBuildingData, originCell, size))
        {
            return false;
        }

        List<Vector3Int> cells = GetOccupiedCells(originCell, size);

        // 구매 배치 모드에서는 null
        // 재배치 모드에서는 자신의 기존 점유만 무시
        return area.AreCellsAvailable(cells, selectedPlacedBuilding);
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
    private Vector3 GetBuildingCenter(Grid targetGrid, Vector3Int originCell, Vector2Int size)
    {
        Vector3Int lastCell = originCell + new Vector3Int(size.x - 1, size.y - 1, 0);

        Vector3 firstCenter = targetGrid.GetCellCenterWorld(originCell);
        Vector3 lastCenter = targetGrid.GetCellCenterWorld(lastCell);

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

    private void ChangeMode(PlacementMode nextMode)
    {
        if (CurrentMode == nextMode) return;

        CurrentMode = nextMode;
        ModeChanged?.Invoke(CurrentMode);
    }

    private void TryConfirmPurchase()
    {
        if (CurrentMode != PlacementMode.PurchasePlacement ||
            selectedBuildingData == null)
        {
            return;
        }

        if (currentArea == null || !canPlace)
        {
            AudioManager.Instance.PlaySFX(ESFXType.ImpossibleBuild);
            return;
        }

        BuildingPurchaseStatus status = EvaluatePurchase(selectedBuildingData);

        if (!status.CanPurchase) return;

        Vector2Int rotatedSize = GetRotatedSize(selectedBuildingData.Size, rotationIndex);

        List<Vector3Int> cells = GetOccupiedCells(currentCell, rotatedSize);

        if (!IsCellsAvailable(currentArea, currentCell, rotatedSize))
        {
            AudioManager.Instance.PlaySFX(ESFXType.ImpossibleBuild);
            UpdatePreview();
            return;
        }

        Vector3 buildingPos = GetBuildingCenter(currentArea.Grid, currentCell, rotatedSize);

        GameObject buildingObj = Instantiate(
            selectedBuildingData.BuildingPrefab,
            buildingPos,
            GetRotation(rotationIndex),
            buildingContainer
        );

        if (!buildingObj.TryGetComponent(out PlacedBuilding placedBuilding))
        {
            Destroy(buildingObj);
            return;
        }

        placedBuilding.Initialize(
                selectedBuildingData,
                currentArea,
                currentCell,
                rotationIndex,
                cells
        );

        if (!currentArea.TryOccupy(placedBuilding, cells))
        {
            Destroy(buildingObj);
            UpdatePreview();
            return;
        }

        if (!CurrencySystem.Instance.TrySpendMoney(status.FinalCost))
        {
            currentArea.Release(placedBuilding, cells);
            Destroy(buildingObj);
            return;
        }

        buildingObj.name = selectedBuildingData.BuildingName;

        // 시설 배치 효과음 
        AudioManager.Instance.PlaySFX(ESFXType.CanBuild);
        placedBuilding.BeginConstruction();

        OnBuildingPlaced?.Invoke(placedBuilding, selectedBuildingData);

        UpdatePreview();
    }

    private void ClearPurchaseRuntime()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }

        previewObject = null;
        selectedBuildingData = null;
        selectedPlacedBuilding = null;
        currentArea = null;
        rotationIndex = 0;
        canPlace = false;

        SelectionChanged?.Invoke(null);
    }

    // 게임 시작 시 배치되어 있는 시설을 점유셀에 등록하는 메서드
    private void RegisterPreplacedBuildings()
    {
        if (buildingContainer == null) return;

        PlacedBuilding[] buildings = buildingContainer.GetComponentsInChildren<PlacedBuilding>(true);

        foreach (PlacedBuilding building in buildings)
        {
            if (building == null || building.Data == null || building.AssignedArea != null)
            {
                continue;
            }

            TryRegisterPreplacedBuildings(building);
        }
    }

    private bool TryRegisterPreplacedBuildings(PlacedBuilding building)
    {
        short buildingRotationIndex = (short)(Mathf.RoundToInt(building.transform.eulerAngles.y / 90f) % 4);

        Vector2Int rotatedSize = GetRotatedSize(building.Data.Size, buildingRotationIndex);

        foreach (BuildableArea area in buildableAreas)
        {
            if (area == null || area.Grid == null || !area.IsBuildableAllowed(building.Data))
            {
                continue;
            }

            Vector3Int centerCell = area.Grid.WorldToCell(building.transform.position);

            centerCell.z = 0;

            Vector3Int originCell = centerCell - new Vector3Int(rotatedSize.x / 2, rotatedSize.y / 2, 0);

            if (!area.CanPlaceBuilding(building.Data, originCell, rotatedSize))
            {
                continue;
            }

            List<Vector3Int> cells = GetOccupiedCells(originCell, rotatedSize);

            if (!area.TryOccupy(building, cells))
            {
                continue;
            }

            building.ApplyPlacement(
                area,
                originCell,
                buildingRotationIndex,
                cells,
                building.transform.position,
                building.transform.rotation
            );

            building.CompletePreplacedBuilding();

            return true;
        }

        Debug.LogWarning($"사전 배치 시설 등록에 실패했습니다 : {building.BuildingName}");
        return false;
    }

    public void SetBuildModeActive(bool active)
    {
        IsBuildModeActive = active;

        if (!active && CurrentMode != PlacementMode.None)
        {
            ExitCurrentMode();
        }
    }
    #endregion

    #region 저장 및 복구

    // 현재 BuildingContainer 아래에 배치된 모든 시설을 반환
    public PlacedBuilding[] GetPlacedBuildings()
    {
        if (buildingContainer == null)
        {
            Debug.LogError("PlacementSystem에 Building Container가 연결되지 않았습니다.");
            return Array.Empty<PlacedBuilding>();
        }

        return buildingContainer.GetComponentsInChildren<PlacedBuilding>();
    }

    // 저장 데이터를 불러오기 전 현재 월드의 모든 시설을 제거
    public void ClearBuildingsOnLoad()
    {
        if (buildingContainer == null)
        {
            Debug.LogError("PlacementSystem에 Building Container가 연결되지 않았습니다.");
            return;
        }

        // 건설, 재배치, 판매 모드 중이라면 종료
        // 프리뷰 참조가 남을 수 있는 문제 방지
        if (CurrentMode != PlacementMode.None)
        {
            ExitCurrentMode();
        }

        PlacedBuilding[] buildings = GetPlacedBuildings();

        foreach (PlacedBuilding building in buildings)
        {
            if (building == null) continue;

            BuildableArea assignedArea = building.AssignedArea;

            if (assignedArea != null)
            {
                assignedArea.Release(building, building.OccupiedCells);
            }

            // 처리된 시설은 컨테이너에서 분리해 재검사 대상이 되지 않도록 정리 
            building.transform.SetParent(null, false);
            building.gameObject.SetActive(false);

            Destroy(building.gameObject);
        }

        selectedPlacedBuilding = null;
        currentArea = null;

        SelectionChanged?.Invoke(null);
    }

    public bool TryGetBuildableArea(string areaId, out BuildableArea result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(areaId))
        {
            return false;
        }

        foreach (BuildableArea area in buildableAreas)
        {
            if (area == null) continue;

            if (string.Equals(area.AreaId, areaId, StringComparison.Ordinal))
            {
                result = area;
                return true;
            }
        }

        return false;
    }

    public bool TryRestoreBuilding(
        FacilitySaveData saved,
        BuildingDataSO data,
        BuildableArea area,
        out PlacedBuilding result)
    {
        result = null;

        if (saved == null ||
            data == null ||
            area == null ||
            saved.rotationIndex > 3)
        {
            return false;
        }

        Vector3Int originCell = new(saved.originCellX, saved.originCellY, 0);
        Vector2Int rotatedSize = GetRotatedSize(data.Size, saved.rotationIndex);
        List<Vector3Int> cells = GetOccupiedCells(originCell, rotatedSize);

        if (!area.CanPlaceBuilding(data, originCell, rotatedSize) || !area.AreCellsAvailable(cells))
        {
            return false;
        }

        Vector3 worldPosition = GetBuildingCenter(area.Grid, originCell, rotatedSize);

        Quaternion worldRotation = GetRotation((short)saved.rotationIndex);

        GameObject instance = Instantiate(data.BuildingPrefab, worldPosition, worldRotation, buildingContainer);

        if (!instance.TryGetComponent(out PlacedBuilding building))
        {
            Destroy(instance);
            return false;
        }

        bool initialized =
            building.InitializeOnLoad(
            data,
            area,
            originCell,
            saved.rotationIndex,
            cells,
            saved.guid,
            saved.placementSource,
            saved.buildingState,
            saved.constructionProgress01);

        if (!initialized || !area.TryOccupy(building, cells))
        {
            Destroy(instance);
            return false;
        }

        instance.name = data.BuildingName;
        result = building;

        OnBuildingPlaced?.Invoke(building, data);

        return true;
    }
    
    #endregion

    #region 이벤트 핸들러
    // 좌클릭 처리
    private void HandlePrimaryClick()
    {
        switch (CurrentMode)
        {
            case PlacementMode.PurchasePlacement:
                TryConfirmPurchase();
                break;

            case PlacementMode.RelocateSelect:
                TrySelectRelocateTarget();
                break;

            case PlacementMode.RelocatePlacement:
                TryConfirmRelocation();
                break;

            case PlacementMode.SellSelect:
                TrySelectSellTargetPointer();
                break;

            case PlacementMode.SellConfirm:
                break; 
        }
    }

    // 우클릭 처리
    private void HandleSecondaryClick()
    {
        if (CurrentMode == PlacementMode.PurchasePlacement || 
            CurrentMode == PlacementMode.RelocatePlacement)
        {
            RotatePreview();
        }
    }

    private void HandleBuildingLongPressed(PlacedBuilding building)
    {
        if (!IsBuildModeActive)
        {
            return;
        }

        if (CurrentMode == PlacementMode.None)
        {
            BeginRelocateMode();
        }

        if (CurrentMode != PlacementMode.RelocateSelect)
        {
            return;
        }

        TryBeginRelocate(building);
    }
    #endregion
}
