using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//버튼의 Icon 색상 함께 변경
[RequireComponent(typeof(Button))]
public class UIButtonChildColorTint : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Button targetButton;
    [SerializeField] private Graphic childGraphics;

    void Awake()
    {
        targetButton = GetComponent<Button>();

        if(childGraphics == null)
        {
            childGraphics = GetComponentInChildren<Graphic>();
        }
    }

    private void ApplyColor(Color color)
    {
        if (childGraphics == null) return;

        Color newColor = color;
        newColor.a = childGraphics.color.a;
        childGraphics.color = newColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (targetButton.interactable) ApplyColor(targetButton.colors.pressedColor);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (targetButton.interactable) ApplyColor(targetButton.colors.normalColor);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetButton.interactable) ApplyColor(targetButton.colors.highlightedColor);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetButton.interactable) ApplyColor(targetButton.colors.normalColor);
    }
    public void OnSelect(BaseEventData eventData)
    {
        if (targetButton.interactable) ApplyColor(targetButton.colors.selectedColor);
    }
    public void OnDeselect(BaseEventData eventData)
    {
        if (targetButton.interactable) ApplyColor(targetButton.colors.normalColor);
    }
}