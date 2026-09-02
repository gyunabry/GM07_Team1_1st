using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarningPopupController : MonoBehaviour
{
    [Header("팝업")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField, TextArea]
    private string warningDescription = "공간이 부족해 삭제되는 아이템이 있습니다.\n +" +
        "그래도 판매하시겠습니까?";

    private Action continueSellAction;

    public bool isOpen => gameObject.activeSelf;

    public void ShowPopup(Action onContinueSale)
    {
        if (onContinueSale == null)
        {
            Debug.LogWarning("판매 처리 메서드가 지정되지 않았습니다.");
            return;
        }

        if (isOpen) return;

        continueSellAction = onContinueSale;

        if (messageText != null)
        {
            messageText.text = warningDescription;
        }

        gameObject.SetActive(true);
    }

    public void OnClickContinueSale()
    {
        AudioManager.Instance?.PlaySFX(ESFXType.UI_ButtonClick);

        Action action = continueSellAction;

        ClosePopup();

        action?.Invoke();
    }

    public void OnClickCancelSale()
    {
        AudioManager.Instance?.PlaySFX(ESFXType.UI_ButtonClick);

        ClosePopup();
    }

    private void ClosePopup()
    {
        if (!isOpen)
        {
            return;
        }

        continueSellAction = null;
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        continueSellAction = null;
    }
}
