using TMPro;
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

    [Header("버튼 설정")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text facilityName;
    [SerializeField] private TMP_Text facilityCount;
    [SerializeField] private TMP_Text facilityPrice;

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip.ShowTooltip(buildingData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.HideTooltip();
    }

    public void RefreshInteractable(int currentMoney)
    {
        // button.interactable = currentMoney >= buildingData.BuildCost;
    }
}
