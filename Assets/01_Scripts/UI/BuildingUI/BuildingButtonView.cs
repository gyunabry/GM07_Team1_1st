using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 각 하단 건물 버튼에 추가할 컴포넌트
// IPointerEnterHandler, IPointerExitHandler를 구현해 툴팁 활성화/비활성화

public class BuildingButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("툴팁 설정")]
    [SerializeField] private BuildingInfoTooltip tooltip;
    [SerializeField] private BuildingDataSO buildingData;

    [Header("버튼 정보")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text facilityName;
    [SerializeField] private TMP_Text facilityCount;
    [SerializeField] private TMP_Text facilityPrice;

    [Header("색상")]
    [SerializeField] private Color disableColor = Color.red;

    private Color defaultNameColor;
    private Color defaultPriceColor;

    private void Awake()
    {
        if (facilityName != null) defaultNameColor = facilityName.color;
        if (facilityPrice != null) defaultPriceColor = facilityPrice.color;
    }

    private void Start()
    {
        RefreshView();

        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.CurrencyChanged += HandleCurrencyChanged;
        }

        if (FacilityManager.Instance != null)
        {
            FacilityManager.Instance.FacilityCountChanged += HandleFacilityCountChanged;
        }
    }

    private void OnDestroy()
    {
        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.CurrencyChanged -= HandleCurrencyChanged;
        }

        if (FacilityManager.Instance != null)
        {
            FacilityManager.Instance.FacilityCountChanged -= HandleFacilityCountChanged;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip.ShowTooltip(buildingData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.HideTooltip();
    }

    private void HandleCurrencyChanged(int money, int exp)
    {
        RefreshInteractable(money);
    }

    private void HandleFacilityCountChanged(BuildingDataSO data, int currentCount)
    {
        if (data == buildingData)
        {
            RefreshView();
        }
    }

    private void RefreshView()
    {
        if (buildingData == null) return;

        if (facilityName != null) facilityName.text = buildingData.BuildingName;
        if (facilityPrice != null) facilityPrice.text = $"{buildingData.BuildCost:N0}G";

        // FacilityManager에 저장된 시설별 개수를 저장
        int placedCount = FacilityManager.Instance != null ? FacilityManager.Instance.GetPlacedCount(buildingData) : 0;

        if (facilityCount != null)
        {
            facilityCount.text = $"{placedCount} / 3"; // TODO: 최대 배치 가능 개수 정의 필요
        }

        int currentMoney = CurrencySystem.Instance != null ? CurrencySystem.Instance.Money : 0;

        RefreshInteractable(currentMoney);
    }

    public void RefreshInteractable(int currentMoney)
    {
        if (buildingData == null) return;

        // 현재 소지 금액이 해당 시설을 짓는데 충분한지 검사
        bool hasEnoughMoney = CurrencySystem.Instance != null && currentMoney >= buildingData.BuildCost;
        // 해당 시설의 배치가능 수가 남아있는지 검사
        bool hasPlacementSlot = FacilityManager.Instance != null && FacilityManager.Instance.CanPlace(buildingData);

        if (button != null) button.interactable = hasEnoughMoney && hasPlacementSlot;

        if (facilityName != null)
        {
            facilityName.color = hasEnoughMoney ? defaultNameColor : disableColor;
        }
        if (facilityPrice != null)
        {
            facilityPrice.color = hasEnoughMoney ? defaultNameColor : disableColor;
        }
    }
}