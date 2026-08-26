using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    [Header("Volume Sliders")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Screen Mode")]
    [SerializeField] private TMP_Dropdown dropdown;

    public enum EScreenMode
    {
        FullScreen,
        MaximizedWindow,
        Windowed,
    }

    private void Awake()
    {
        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
        this.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        float currentBGM = 1f;
        float currentSFX = 1f;

        if (AudioManager.Instance != null)
        {
            currentBGM = AudioManager.Instance.GetBGMVolume();
            currentSFX = AudioManager.Instance.GetSFXVolume();
        }

        if (bgmSlider != null)
        {
            bgmSlider.value = currentBGM;
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = currentSFX;
        }

        OnClickChangeWindow();
    }

    // 클릭 시 효과음
    public void OnClickSFXSound()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);
    }

    // 옵션 열기
    public void OnClickOpenOption()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_Open);
        this.gameObject.SetActive(true);
    }

    // 가이드 열기
    public void OnClickHelpBtn()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);
    }

    // 크레딧 열기
    public void OnClickCreditBtn()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);
    }

    // 메인화면으로 돌아가기
    public void OnClickHomeBtn()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);
        GameSceneManager.Instance.LoadScene(EScene.Title);
    }

    // 화면 크기 변경
    public void OnClickChangeWindow()
    {
        List<string> options = new List<string>
        {
            "전체화면",
            "전체창화면",
            "창화면"
        };

        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.onValueChanged.AddListener(index => ChangeFullScreenMode((EScreenMode)index));

        switch (dropdown.value)
        {
            case 0:
                Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow); break;
            case 1:
                Screen.SetResolution(1920, 1080, FullScreenMode.MaximizedWindow); break;
            case 2:
                Screen.SetResolution(1920, 1080, FullScreenMode.Windowed); break;
        }
    }

    private void ChangeFullScreenMode(EScreenMode mode)
    {
        switch (mode)
        {
            case EScreenMode.FullScreen:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow; break;
            case EScreenMode.MaximizedWindow:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow; break;
            case EScreenMode.Windowed:
                Screen.fullScreenMode = FullScreenMode.Windowed; break;
        }
    }

    private void OnBGMVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(value);
        }
        PlayerPrefs.SetFloat("BGM_Volume", value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
        PlayerPrefs.SetFloat("SFX_Volume", value);
    }
}
