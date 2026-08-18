using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInteractionPanel : MonoBehaviour
{
    [Header("캔버스")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private GraphicRaycaster graphicRaycaster;

    [Header("시설 공통 정보")]
    [SerializeField] private TMP_Text buildingNameText;

    [Header("시설별 상세 뷰")]
    [SerializeField] private BuildingDetailView[] detailViews;

    private BuildingDetailView activeView;

    private void Awake()
    {
        DeactiveAllViews();
        SetVisible(false);
    }

    private void OnDestroy()
    {
        ReleaseActiveView();
    }

    public void ShowPanel(IBuildingUIModel building)
    {
        if (building == null) return;

        // 다른 시설을 선택했을 때 이전 시설 해제
        ReleaseActiveView();

        buildingNameText.text = building.BuildingName;

        Component buildingComponent = building as Component;
        ProductionBuilding productionBuilding = null;

        foreach (BuildingDetailView view in detailViews)
        {
            if (view == null || !view.Supports(building)) continue;

            activeView = view;
            activeView.gameObject.SetActive(true);
            activeView.Bind(building);
            break;
        }

        SetVisible(true);
    }

    public void HidePanel()
    {
        ReleaseActiveView();
        SetVisible(false);
    }

    private void ReleaseActiveView()
    {
        if (activeView == null) return;

        activeView.Unbind();
        activeView.gameObject.SetActive(false);
        activeView = null;
    }

    private void DeactiveAllViews()
    {
        if (detailViews != null)
        {
            foreach (BuildingDetailView view in detailViews)
            {
                if (view != null)
                {
                    view.gameObject.SetActive(false);
                }
            }
        }
    }

    private void SetVisible(bool visible)
    {
        targetCanvas.enabled = visible;
        graphicRaycaster.enabled = visible;
    }
}
