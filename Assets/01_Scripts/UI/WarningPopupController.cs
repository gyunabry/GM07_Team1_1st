using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WarningPopupController : MonoBehaviour
{
    [Header("ÆË¾÷")]
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private TMP_Text messageText;

    public bool isOpen => popupRoot != null && popupRoot.activeSelf;

    public void ShowMessage(string msg)
    {
        if (popupRoot == null || string.IsNullOrWhiteSpace(msg))
        {
            return;
        }

        if (messageText != null)
        {
            messageText.text = msg;
        }

        popupRoot.SetActive(true);
    }

    public void OnClickConfirm()
    {
        AudioManager.Instance?.PlaySFX(ESFXType.UI_ButtonClick);

        ClosePopup();
    }

    public void ClosePopup()
    {
        if (popupRoot != null)
        {
            popupRoot.SetActive(false);
        }
    }
}
