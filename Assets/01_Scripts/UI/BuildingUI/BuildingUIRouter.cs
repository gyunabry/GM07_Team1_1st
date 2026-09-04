using UnityEngine;

// BuildingDetailView를 구현하는 각 시설 뷰를 담고 보여주는 라우터 클래스

public class BuildingUIRouter : MonoBehaviour
{
    [SerializeField] private BuildingDetailView[] views;

    // 현재 활성화된 뷰
    private BuildingDetailView activeView;

    public bool Open(IBuildingUIModel building)
    {
        Close();

        if (building == null) return false;

        foreach (BuildingDetailView view in views)
        {
            if (view == null || !view.Supports(building))
            {
                continue;
            }

            activeView = view;
            AudioManager.Instance.PlaySFX(ESFXType.UI_Open);
            activeView.Open(building);

            return true;
        }

        Debug.LogWarning($"{building.BuildingName}을 지원하는 시설 UI가 없습니다.");
        return false;
    }

    public void Close()
    {
        if (activeView == null) return;
        AudioManager.Instance.PlaySFX(ESFXType.UI_Close);

        activeView.Close();
        activeView = null;
    }
}
