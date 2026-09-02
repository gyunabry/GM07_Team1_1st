using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BuildTapController : MonoBehaviour
{
    [Serializable]
    private class TabBinding
    {
        public Button button;
        public GameObject content;
        public GameObject highlight;
    }

    [SerializeField] private TabBinding[] tabs;
    [SerializeField] private int defaultTabIndex = 0;

    private UnityAction[] clickActions;

    public int CurrentTabIndex { get; private set; } = -1;

    private void Awake()
    {
        clickActions = new UnityAction[tabs.Length];

        for (int i = 0; i < tabs.Length; i++)
        {
            int capturedIndex = i;

            clickActions[i] = () => OnTabClicked(capturedIndex);

            if (tabs[i].button != null)
            {
                tabs[i].button.onClick.AddListener(clickActions[i]);
            }
        }

        ShowTab(Mathf.Clamp(defaultTabIndex, 0, tabs.Length - 1));
    }

    private void OnTabClicked(int index)
    {
        if (index == CurrentTabIndex)
        {
            return;
        }

        AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);
        ShowTab(index);
    }

    public void ShowTab(int index)
    {
        if (index < 0 || index >= tabs.Length)
        {
            return;
        }

        CurrentTabIndex = index;

        for (int i = 0; i < tabs.Length; i++)
        {
            bool isSelected = i == index;

            if (tabs[i].content != null)
            {
                tabs[i].content.SetActive(isSelected);
            }

            if (tabs[i].highlight != null)
            {
                tabs[i].highlight.SetActive(isSelected);
            }
        }
    }

    private void OnDestroy()
    {
        if (clickActions == null)
        {
            return;
        }

        for (int i = 0; i < tabs.Length; i++)
        {
            if (tabs[i].button != null && clickActions[i] != null)
            {
                tabs[i].button.onClick.RemoveListener(clickActions[i]);
            }
        }
    }
}
