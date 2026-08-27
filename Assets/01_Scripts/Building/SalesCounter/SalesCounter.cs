using System;
using UnityEngine;

/*
[판매 흐름]
1. 플레이어가 판매대 안쪽 상호작용 영역에 서면 아이템이 창고로 이동 (자동 운반 직원 동일?)
2. 손님이 판매대 앞에 도착
3. 원하는 상품이 없다면 인내 시간 동안 대기
4. 상품과 판매 담당자가 있다면 판매 시간 시작
5. 일정 시간 동안 조건이 유지되면 요구 상품을 차감하고 돈과 경험치 지급
6. 담당자가 판매 위치를 벗어나거나 재고가 사라지면 판매 중단 (판매 진행도는 초기화)
 */

// 판매대 배치 시 OperationArea에 직원 자동 배치

public class SalesCounter : MonoBehaviour
{
    [Header("고객 시스템")]
    [SerializeField] private CustomerCheckoutStation checkoutStation;
    [SerializeField] private PlacementSystem placementSystem;

    private PlacedBuilding placedBuilding;
    private CounterInventory counterInventory;

    public ItemInventory Inventory => 
        CounterInventory.Instance != null 
        ? CounterInventory.Instance.Inventory 
        : null;

    // 건설이 완료되어야 판매대 운영 가능
    public bool CanOperate => placedBuilding != null && placedBuilding.IsComplete;

    public bool IsOpen => checkoutStation != null && checkoutStation.IsAvailable;

    public event Action StateChanged;

    private void Awake()
    {
        placedBuilding = GetComponent<PlacedBuilding>();
        checkoutStation = GetComponentInChildren<CustomerCheckoutStation>(true);
        placementSystem ??= FindAnyObjectByType<PlacementSystem>();

        if (placedBuilding != null)
        {
            placedBuilding.OnStateChanged += HandleBuildingStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (placedBuilding != null)
        {
            placedBuilding.OnStateChanged -= HandleBuildingStateChanged;
        }
    }

    private void OnEnable()
    {
        if (placementSystem != null)
        {
            placementSystem.OnBuildingMoved += HandleBuildingMoved;
        }
    }

    private void OnDisable()
    {
        if (placementSystem != null)
        {
            placementSystem.OnBuildingMoved -= HandleBuildingMoved;
        }
    }
    private void Start()
    {
        counterInventory = CounterInventory.Instance;

        if (counterInventory == null)
        {
            Debug.LogError("전역 판매대 인벤토리가 필요합니다.");
            checkoutStation?.CloseStation();
            return;
        }

        if (checkoutStation == null)
        {
            Debug.LogError("체크아웃 스테이션이 필요합니다.");
            return;
        }
    }

    // 판매대 이동, 회전, 철거 전에 호출
    public void CloseCounter()
    {
        bool wasOpen = IsOpen;

        checkoutStation?.CloseStation();

        if (wasOpen != IsOpen)
        {
            StateChanged?.Invoke();
        }
    }

    // 이동, 회전, NavMesh 반영이 끝나고 호출
    public bool OpenCounter()
    {
        if (!CanOperate || checkoutStation == null)
        {
            return false;
        }

        bool wasOpen = IsOpen;

        checkoutStation.OpenStation();

        if (wasOpen != IsOpen)
        {
            StateChanged?.Invoke();
        }

        return true;
    }

    private void Reset()
    {
        checkoutStation = GetComponentInChildren<CustomerCheckoutStation>(true);
    }

    private void HandleBuildingStateChanged()
    {
        StateChanged?.Invoke();
    }
    private void HandleBuildingMoved(PlacedBuilding building)
    {
        if (building != placedBuilding || !CanOperate)
        {
            return;
        }

        // 기존 손님은 이전 판매대 위치를 목적지로 갖고 있으므로 대기열을 비운다.
        // 판매대를 다시 열면 CustomerSpawnManager가 새 위치 기준으로 손님을 채운다.
        CloseCounter();
        OpenCounter();
    }
}
