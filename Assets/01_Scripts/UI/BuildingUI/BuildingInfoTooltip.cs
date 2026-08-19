using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildingInfoTooltip : MonoBehaviour
{
    [Header("툴팁")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private Canvas canvas; // 부모 캔버스
    [SerializeField] private Vector2 mouseOffset;

    [Header("건물 정보")]
    [SerializeField] private Image buildingIcon;
    [SerializeField] private TMP_Text buildingName;
    [SerializeField] private TMP_Text buildingPrice;
    [SerializeField] private TMP_Text buildingCount;
    [SerializeField] private TMP_Text buildingDescription;
    [SerializeField] private TMP_Text buildingType;

    private RectTransform tooltipRect;
    private RectTransform tooltipParentRect;

    private void Awake()
    {
        tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        tooltipParentRect = tooltipRect.parent as RectTransform;
    }

    private void Start()
    {
        HideTooltip();
    }

    private void LateUpdate()
    {
        // 활성화된 동안 마우스 위치 추적
        if (tooltipPanel.activeInHierarchy) CalculateMousePosition();
    }

    public void ShowTooltip(BuildingDataSO data)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
        }

        if (buildingIcon != null)
        {
            // buildingIcon.sprite = data.BuildingIcon;
        }

        if (buildingName != null)
        {
            buildingName.text = data.BuildingName;
        }

        if (buildingPrice != null)
        {
            buildingPrice.text = $"{data.BuildCost:N0}G";
        }

        if (buildingDescription != null)
        {
            buildingDescription.text = data.Description;
        }

        if (buildingCount != null)
        {
            FacilityManager manager = FacilityManager.Instance;

            if (manager != null)
            {
                int currentCount = manager.GetPlacedCount(data);
                int maxCount = 3; // TODO: 추후 시설별 최대 배치 가능 수 정의 필요

                buildingCount.text = $"{currentCount} / {maxCount}";
            }
            else
            {
                buildingCount.text = "- / -";
            }
        }

        if (buildingType != null)
        {
            // buildingType.text = data.BuildingType.ToString();
        }
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    private void CalculateMousePosition()
    {
        Vector2 localPosition; // 변환된 canvas내 현재 좌표
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // 마우스 좌표를 canvas 내에서의 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tooltipParentRect, 
            mousePosition, 
            canvas.worldCamera, 
            out localPosition
        );

        // 위치 변경
        tooltipRect.anchoredPosition = localPosition + mouseOffset;
    }
}
