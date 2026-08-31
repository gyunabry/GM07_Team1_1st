using System;
using UnityEngine;

// 시설마다 영구적으로 저장되는 GUID를 제공하는 클래스

[DisallowMultipleComponent]
public class PersistentGuid : MonoBehaviour
{
    [SerializeField] private string value;

    public string Value => value;

    public bool HasValue => Guid.TryParseExact(value, "N", out _);

    // 플레이어가 새 시설을 배치했을 때 호출
    public void AssignNew()
    {
        value = Guid.NewGuid().ToString("N");
    }

    // 저장 데이터로 시설을 복원할 때 기존 GUID를 주입
    public bool TryRestore(string saveGuid)
    {
        if (!Guid.TryParseExact(saveGuid, "N", out Guid parsed))
        {
            return false;
        }

        value = parsed.ToString("N");
        return true;
    }

#if UNITY_EDITOR
    [ContextMenu("영구 GUID 발급")]
    private void GenerateGuid()
    {
        AssignNew();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
