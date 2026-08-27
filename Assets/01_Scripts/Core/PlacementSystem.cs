using System;
using System.Collections.Generic;
using UnityEngine;

/*
 상태 패턴 활용 리팩토링 예정
- Placement
- Remove
- Interior
 */

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

    // 셀별로 어떤 건물이 점유 중인지 저장
    //private readonly Dictionary<Vector3Int, PlacedBuilding> occupiedCells = new();

    // 현재 배치 대상으로 선택된 건물 데이터
    private BuildingDataSO selectedBuildingData;
    private PlacedBuilding selectedPlacedBuilding;
    private GameObject previewObject;
    private Vector3Int currentCell;

    // 0 : 0도 / 1: 90도 / 2: 180도 / 3: 270도
    private short rotationIndex;
    private bool canPlace;

    public bool IsBuildModeActive { get; private set; }

    [field: SerializeField]
    public PlacementMode CurrentMode { get; private set; }
    public PlacedBuilding SelectedPlacedBuilding => selectedPlacedBuilding;

    public bool ConsumeWorldInput => CurrentMode != PlacementMode.None;

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

        // 이벤트로 입력 처리
        //inputManager.OnClicked += PlaceBuilding;
        //inputManager.OnExit += CancelPlacement;
        //inputManager.OnRotation += RotatePreview;

        inputManager.OnPrimaryClicked += HandlePrimaryClick;
        inputManager.OnSecondaryClicked += HandleSecondaryClick;
        inputManager.OnCancelPressed += CancelCurrentAction;
        inputManager.OnBuildingLongPressed += HandleBuildingLongPressed;
    }

    private void OnDisable()
    {
        if (inputManager == null) return;

        //inputManager.OnClicked -= PlaceBuilding;
        //inputManager.OnExit -= CancelPlacement;
        //inputManager.OnRotation -= RotatePreview;

        inputManager.OnPrimaryClicked -= HandlePrimaryClick;
        inputManager.OnSecondaryClicked -= HandleSecondaryClick;
        inputManager.OnCancelPressed -= CancelCurrentAction;
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
        //if (buildingData == null)
        //{
        //    Debug.LogWarning("배치할 건물 데이터가 지정되지 않았습니다.");
        //    return;
        //}

        //// 기존 선택된 건물 배치를 취소하기 위해 Cancel 호출
        //CancelPlacement();

        //selectedBuildingData = buildingData;
        //rotationIndex = 0;

        //CreatePreview();

        StartPurchasePlacement(data);
    }

    public bool StartPurchasePlacement(BuildingDataSO data)
    {
        //if (CurrentMode != PlacementMode.None || data == null)
        //{
        //    return false;
        //}

        if (data == null) return false;

        BuildingPurchaseStatus status = EvaluatePurchase(data);
        if (!status.CanPurchase)
        {
            return false;
        }

        if (CurrentMode == PlacementMode.PurchasePlacement &&
            selectedBuildingData == data)
        {
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

            case PlacementMode.RelocatePlacement:
                ClearRelocationRuntime();
                ChangeMode(PlacementMode.RelocateSelect);
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

        List<Vector3Int> cells = GetOccupiedCells(originCell, size);

        // 구매 배치 모드에서는 null
        // 재배치 모드에서는 자신의 기존 점유만 무시
        return area.AreCellsAvailable(cells, selectedPlacedBuilding);

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

    // 해당 셀이 이미 점유된 상태인지 반환
    //public bool IsCellOccupied(Vector3Int cell)
    //{
    //    cell.z = 0;
    //    return occupiedCells.ContainsKey(cell);
    //}

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
            currentArea == null ||
            !canPlace ||
            selectedBuildingData == null)
        {
            return;
        }

        BuildingPurchaseStatus status = EvaluatePurchase(selectedBuildingData);

        if (!status.CanPurchase) return;

        Vector2Int rotatedSize = GetRotatedSize(selectedBuildingData.Size, rotationIndex);

        List<Vector3Int> cells = GetOccupiedCells(currentCell, rotatedSize);

        if (!currentArea.AreCellsAvailable(cells))
        {
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

            return true;
        }

        Debug.LogWarning($"사전 배치 시설 등록에 실패했습니다 : {building.BuildingName}");
        return false;
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

    public void SetBuildModeActive(bool active)
    {
        IsBuildModeActive = active;

        if (!active && CurrentMode != PlacementMode.None)
        {
            ExitCurrentMode();
        }
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
