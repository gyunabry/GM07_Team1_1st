using TMPro;
using Unity.Android.Gradle.Manifest;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 각 하단 건물 버튼에 추가할 컴포넌트
// IPointerEnterHandler, IPointerExitHandler를 구현해 툴팁 활성화/비활성화

public class BuildingButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("참조")]
    [SerializeField] private PlacementSystem placementSystem;

    [Header("툴팁 설정")]
    [SerializeField] private BuildingInfoTooltip tooltip;
    [SerializeField] private BuildingDataSO buildingData;

    [Header("버튼 정보")]
    [SerializeField] private Button button;
    [SerializeField] private Image buildingIcon;
    [SerializeField] private TMP_Text facilityName;
    [SerializeField] private TMP_Text facilityCount;
    [SerializeField] private TMP_Text facilityPrice;
    [SerializeField] private Image unavailableImage;

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
            // CurrencySystem.Instance.CurrencyChanged += HandleCurrencyChanged;
            CurrencySystem.Instance.CurrencyChanged_Gold += HandleGoldChanged;
            CurrencySystem.Instance.LevelUp += HandleLevelUp;
        }

        if (FacilityManager.Instance != null)
        {
            FacilityManager.Instance.FacilityInfoChanged += HandleFacilityInfoChanged;
        }
    }

    private void OnDestroy()
    {
        if (CurrencySystem.Instance != null)
        {
            // CurrencySystem.Instance.CurrencyChanged -= HandleCurrencyChanged;
            CurrencySystem.Instance.CurrencyChanged_Gold -= HandleGoldChanged;
            CurrencySystem.Instance.LevelUp -= HandleLevelUp;
        }

        if (FacilityManager.Instance != null)
        {
            FacilityManager.Instance.FacilityInfoChanged -= HandleFacilityInfoChanged;
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

    private void HandleLevelUp()
    {
        RefreshView();
    }

    private void HandleGoldChanged(int money)
    {
        RefreshView();
    }

    private void HandleFacilityInfoChanged(BuildingDataSO data)
    {
        bool isSameBuilding = data == buildingData;

        // 버튼에 할당된 시설과 data의 시설이 같은 태그를 가졌는지 검사
        bool isSameProductionGroup =
            data != null &&
            buildingData != null &&
            data.BuildingTag == BuildingTag.Production &&
            buildingData.BuildingTag == BuildingTag.Production;

        if (isSameBuilding || isSameProductionGroup)
        {
            RefreshView();
        }
    }

    private void RefreshView()
    {
        if (buildingData == null || placementSystem == null) return;

        BuildingPurchaseStatus status = placementSystem.EvaluatePurchase(buildingData);

        if (facilityName != null) facilityName.text = buildingData.BuildingName;

        if (buildingIcon != null)
        {
            buildingIcon.sprite = buildingData.BuildingIcon;
            buildingIcon.enabled = true;
        }

        if (facilityCount != null)
        {
            facilityCount.text = $"{status.CurrentCount} / {status.MaxCount}";
        }

        if (facilityPrice != null) facilityPrice.text = $"{status.FinalCost:N0}G";

        if (button != null) button.interactable = status.CanPurchase;

        bool levelBlocked = (status.BlockReasons & PurchaseBlockReason.Level) != 0;

        bool moneyBlocked = (status.BlockReasons & PurchaseBlockReason.Money) != 0;

        bool limitBlocked = (status.BlockReasons & PurchaseBlockReason.PlacementLimit) != 0;

        if (facilityName != null)
        {
            facilityName.color = levelBlocked || limitBlocked ? disableColor : defaultNameColor;
        }
        if (facilityPrice != null)
        {
            facilityPrice.color = moneyBlocked ? disableColor : defaultNameColor;
        }

        if (unavailableImage != null)
        {
            unavailableImage.gameObject.SetActive(levelBlocked || moneyBlocked || limitBlocked);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left ||
            buildingData == null ||
            placementSystem == null)
        {
            return;
        }

        BuildingPurchaseStatus status = placementSystem.EvaluatePurchase(buildingData);

        if (status.CanPurchase) return;

        ESFXType sound = status.BlockReasons == PurchaseBlockReason.Money
            ? ESFXType.UI_LackGoods
            : ESFXType.ImpossibleBuild;

        AudioManager.Instance.PlaySFX(sound);
    }
}