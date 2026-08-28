using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillTreeScale : MonoBehaviour, IScrollHandler
{
    private RectTransform rectTransform;
    [SerializeField] private float zoomSpeed = 0.01f;
    [SerializeField] private float minZoom = 0.5f;
    [SerializeField] private float maxZoom = 3.0f;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    public void OnScroll(PointerEventData eventData)
    {
        float scrollValue = eventData.scrollDelta.y;
        Vector3 newScale = rectTransform.localScale + Vector3.one * scrollValue * zoomSpeed;
        newScale.x = Mathf.Clamp(newScale.x, minZoom, maxZoom);
        newScale.y = Mathf.Clamp(newScale.y, minZoom, maxZoom);
        newScale.z = 1f;

        rectTransform.localScale = newScale;
    }
}
