using Unity.VisualScripting;
using UnityEngine;

// 이 BuildableArea가 어떤 몬스터의 SpawnArea를 이용하는지 담는 콘텍스트

public class HuntingFieldContext : MonoBehaviour
{
    [SerializeField] private Collider spawnArea;

    private BuildableArea buildableArea;

    public BuildableArea BuildableArea => buildableArea;
    public Collider SpawnArea => spawnArea;
    public bool IsValid => buildableArea != null && spawnArea != null;

    private void Awake()
    {
        buildableArea = GetComponent<BuildableArea>();

        if (spawnArea == null)
        {
            Debug.LogWarning($"{name} 사냥터 콜라이더가 없습니다.");
        }
    }

    // 해당 영역에 전송기가 있는지 검사
    public bool TryGetCompletedTransmitter(out Transmitter transmitter)
    {
        transmitter = null;

        Transmitter[] candidates = FindObjectsByType<Transmitter>(
            FindObjectsInactive.Exclude, 
            FindObjectsSortMode.None
        );

        foreach (Transmitter candidate in candidates )
        {
            if (candidate == null) continue;

            PlacedBuilding transmitterBuilding = candidate.GetComponentInParent<PlacedBuilding>();

            if (transmitterBuilding == null) continue;

            if (transmitterBuilding.AssignedArea != buildableArea)
            {
                continue;
            }

            if (!transmitterBuilding.IsComplete) continue;

            transmitter = candidate;
            return true;
        }

        return false;
    }
}
