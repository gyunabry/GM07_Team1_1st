using UnityEngine;

/// <summary>
/// 공통 시설 상세 패널에 표시되는 시설별 뷰의 기반 클래스
/// </summary>
public abstract class BuildingDetailView : MonoBehaviour
{
    /// <summary>
    /// 전달받은 시설을 View에서 표시할 수 있는지 검사
    /// </summary>
    public abstract bool Supports(IBuildingUIModel building);

    /// <summary>
    /// 시설 데이터를 연결하고 필요한 이벤트를 구독
    /// </summary>
    public abstract void Open(IBuildingUIModel building);

    /// <summary>
    /// 시설 참조를 제거하고 이벤트 구독을 해제
    /// </summary>
    public abstract void Close();

    /// <summary>
    /// 같은 오브젝트에 있는 실제 기능 컴포넌트를 찾아서 반환
    /// </summary>
    protected static T GetBuildingComponent<T>(IBuildingUIModel building) where T : Component
    {
        Component buildingComponent = building as Component;

        return buildingComponent != null ? buildingComponent.GetComponent<T>() : null;
    }
}
