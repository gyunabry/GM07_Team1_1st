using UnityEngine;

public class ToggleUI : MonoBehaviour
{
    [Header("토글 대상")]
    [SerializeField] private GameObject target;

    public void ToggleTarget()
    {
        if (target == null) return;

        target.SetActive(!target.activeSelf);
    }
}
