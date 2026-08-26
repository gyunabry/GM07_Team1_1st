using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    [Header("Volume Sliders")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

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
    }
    public void OnClickOpenOption()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_Open);
        this.gameObject.SetActive(true);
    }

    public void OnClickHelpBtn()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);
    }

    public void OnClickCreditBtn()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);
    }

    public void OnClickHomeBtn()
    {
        AudioManager.Instance.PlaySFX(ESFXType.UI_ButtonClick);
        GameSceneManager.Instance.LoadScene(EScene.Title);
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
