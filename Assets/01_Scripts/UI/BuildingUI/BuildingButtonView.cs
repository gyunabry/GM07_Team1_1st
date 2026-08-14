using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 각 하단 건물 버튼에 추가할 컴포넌트
// IPointerEnterHandler, IPointerExitHandler를 구현해 툴팁 활성화/비활성화

public class BuildingButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button button;
    [SerializeField] private BuildingDataSO buildingData;
    [SerializeField] private BuildingInfoTooltip tooltip;

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
