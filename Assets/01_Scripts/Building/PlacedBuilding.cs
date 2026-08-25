using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public enum BuildingState
{
    Constructing,
    Completed
}

public class PlacedBuilding : MonoBehaviour, IBuildingUIModel
{
    [Header("건축 상태 오브젝트")]
    [SerializeField] private GameObject constructionObject;
    [SerializeField] private GameObject completedObject;

    [Header("테스트용 시설 상태")]
    [SerializeField] private BuildingState state = BuildingState.Constructing;

    // 해당 빌딩이 차지하고 있는 셀
    private readonly List<Vector3Int> occupiedCells = new();

    public BuildingDataSO Data { get; private set; }
    public Vector3Int OriginCell { get; private set; }
    public short RotationIndex { get; private set; }

    public IReadOnlyList<Vector3Int> OccupiedCells => occupiedCells;
    // 해당 시설이 어느 영역에 있는지 담는 프로퍼티
    public BuildableArea AssignedArea { get; private set; }
    public BuildingState State => state;
    public float ConstructionProgress { get; private set; }
    public bool IsComplete => State == BuildingState.Completed;
    public BuildingSelectionVisual SelectionVisual { get; private set; }

    public event Action OnStateChanged;
    public event Action OnPlacementChanged;
    public event Action<PlacedBuilding> OnConstructionCompleted;

    public string BuildingName => Data.BuildingName;

    /// <summary>
    /// 건물 배치 시 초기화하는 메서드.
    /// 배치된 셀, 점유하는 셀, 회전은 배치된 건물 인스턴스가 관리
    /// </summary>
    /// <param name="data">건물 데이터</param>
    /// <param name="originCell">배치 셀 위치</param>
    /// <param name="rotationIndex">회전 인덱스 0 ~ 3</param>
    /// <param name="cells">점유하는 셀</param>
    public void Initialize(
        BuildingDataSO data, 
        BuildableArea assignedArea,
        Vector3Int originCell, 
        short rotationIndex, 
        IEnumerable<Vector3Int> cells
    ) 
    {
        Data = data;
        AssignedArea = assignedArea;
        OriginCell = originCell;
        RotationIndex = rotationIndex;

        occupiedCells.Clear();
        occupiedCells.AddRange(cells);

        // 건설 시작
        // State = BuildingState.Constructing;
        state = BuildingState.Constructing;
        ConstructionProgress = 0f;

        constructionObject?.SetActive(true);
        completedObject?.SetActive(false);

        OnStateChanged?.Invoke();
    }

    public void ApplyPlacement(
        BuildableArea assignedArea,
        Vector3Int originCell,
        short rotationIndex,
        IEnumerable<Vector3Int> cells,
        Vector3 worldPosition,
        Quaternion worldRotation)
    {
        AssignedArea = assignedArea;
        OriginCell = originCell;
        RotationIndex = (short)(rotationIndex % 4);

        occupiedCells.Clear();

        if (cells != null)
        {
            occupiedCells.AddRange(cells);
        }

        transform.SetPositionAndRotation(worldPosition, worldRotation);

        OnPlacementChanged?.Invoke();
    }

    public void BeginConstruction()
    {
        ConstructAsync(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTask ConstructAsync(CancellationToken cancellationToken)
    {
        float buildTime = Mathf.Max(0f, Data.BuildTime);

        if (buildTime > 0f)
        {
            float elapsedTime = 0f;

            while (elapsedTime < buildTime)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

                elapsedTime += Time.deltaTime;
                ConstructionProgress = Mathf.Clamp01(elapsedTime / buildTime);

                OnStateChanged?.Invoke();
            }
        }

        CompleteConstruction();
    }

    private void CompleteConstruction()
    {
        // State = BuildingState.Completed;
        state = BuildingState.Completed;
        ConstructionProgress = 1f;

        constructionObject?.SetActive(false);
        completedObject.SetActive(true);

        OnStateChanged?.Invoke();
        OnConstructionCompleted?.Invoke(this);
    }
}
