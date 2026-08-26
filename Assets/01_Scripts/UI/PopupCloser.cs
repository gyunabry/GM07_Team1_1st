using UnityEngine;
using UnityEngine.EventSystems;

public class PopupCloser : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject popup;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (popup != null)
        {
            AudioManager.Instance.PlaySFX(ESFXType.UI_Close);
            popup.SetActive(false);
        }
    }
}
