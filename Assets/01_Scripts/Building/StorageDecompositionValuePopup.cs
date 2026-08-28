using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StorageDecompositionValuePopup : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button btnMinus10;
    [SerializeField] private Button btnMinus1;
    [SerializeField] private Button btnPlus10;
    [SerializeField] private Button btnPlus1;
    [SerializeField] private Button btnMax;
    [SerializeField] private Button btnMin;

    [SerializeField] private Button btnConfirm;
    [SerializeField] private Button btnCancel;

    private int currentValue = 0;
    private int maxValue = 0;
    private Action<int> onConfirmCallback;

    private void Awake()
    {
        inputField.onValueChanged.AddListener(OnInputValueChanged);
        inputField.onEndEdit.AddListener(OnInputEndEdit);

        if (btnMinus10 != null) btnMinus10.onClick.AddListener(() => AdjustValue(-10));
        if (btnMinus1 != null) btnMinus1.onClick.AddListener(() => AdjustValue(-1));
        if (btnPlus1 != null) btnPlus1.onClick.AddListener(() => AdjustValue(1));
        if (btnPlus10 != null) btnPlus10.onClick.AddListener(() => AdjustValue(10));
        if (btnMax != null) btnMax.onClick.AddListener(SetMaxValue);
        if (btnMin != null) btnMin.onClick.AddListener(SetMinValue);

        if (btnConfirm != null) btnConfirm.onClick.AddListener(OnClickonfirm);
        if (btnCancel != null) btnCancel.onClick.AddListener(ClosePopup);
    }
    public void OpenPopup(int maxCount, Action<int> onConfirm)
    {
        maxValue = Mathf.Max(0, maxCount);
        onConfirmCallback = onConfirm;

        SetClamp(maxValue > 0 ? 1 : 0);

        gameObject.SetActive(true);
    }
    public void AdjustValue(int delta)
    {
        SetClamp(currentValue + delta);
    }
    public void SetMaxValue()
    {
        SetClamp(maxValue);
    }
    public void SetMinValue()
    {
        SetClamp(0);
    }
    private void OnInputValueChanged(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        if (int.TryParse(text, out int textValue))
        {
            if (textValue < 0 || textValue > maxValue)
            {
                SetClamp(textValue);
            }
            else
            {
                currentValue = textValue;
            }
        }
    }
    private void OnInputEndEdit(string text)
    {
        if(string.IsNullOrEmpty(text) || !int.TryParse(text, out _))
        {
            SetClamp(0);
        }
        else
        {
            SetClamp(int.Parse(text));
        }
    }
    private void OnClickonfirm() 
    {
        if(currentValue <= 0)
        {
            return;
        }

        onConfirmCallback?.Invoke(currentValue);
        ClosePopup();
    }
    private void SetClamp(int newValue)
    {
        currentValue = Mathf.Clamp(newValue, 0, maxValue);
        inputField.SetTextWithoutNotify(currentValue.ToString());
    }
    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }
}
