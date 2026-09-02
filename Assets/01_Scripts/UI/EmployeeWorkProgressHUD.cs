using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 직원이 시간 기반 작업을 수행하는 동안 머리 위에 상품 작업 진행도를 표시합니다.
/// </summary>
[DisallowMultipleComponent]
public sealed class EmployeeWorkProgressHUD : MonoBehaviour
{
    private GameObject hudInstance;
    private Slider progressSlider;
    private bool isPositionAdjusted;

    public void ShowProgress(float normalizedProgress)
    {
        EnsureHud();
        if (hudInstance == null)
        {
            return;
        }

        if (!hudInstance.activeSelf)
        {
            hudInstance.SetActive(true);
        }

        if (progressSlider != null)
        {
            progressSlider.SetValueWithoutNotify(Mathf.Clamp01(normalizedProgress));
        }
    }

    public void Hide()
    {
        if (hudInstance != null)
        {
            hudInstance.SetActive(false);
        }
    }

    private void OnDisable()
    {
        Hide();
    }

    private void EnsureHud()
    {
        if (hudInstance != null)
        {
            return;
        }

        hudInstance = FindChildHud("UI_ProductHUD");
        if (hudInstance == null)
        {
            return;
        }

        RectTransform rectTransform = hudInstance.GetComponent<RectTransform>();
        if (rectTransform != null && !isPositionAdjusted)
        {
            rectTransform.anchoredPosition += Vector2.up * 0.7f;
            isPositionAdjusted = true;
        }

        progressSlider = hudInstance.GetComponentInChildren<Slider>(true);
        SetChildActive("Inven_Icon", false);
        SetChildActive("Inven_Text", false);
        hudInstance.SetActive(false);
    }

    private GameObject FindChildHud(string hudName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != transform && children[i].name == hudName)
            {
                return children[i].gameObject;
            }
        }

        return null;
    }

    private void SetChildActive(string childName, bool isActive)
    {
        Transform[] children = hudInstance.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name == childName)
            {
                children[i].gameObject.SetActive(isActive);
            }
        }
    }
}
