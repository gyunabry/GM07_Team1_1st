using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("AudioSource")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("AudioData")]
    [SerializeField] private AudioData audioData;

    private Dictionary<EBGMType, BGMClipData> bgmDictionary;
    private Dictionary<ESFXType, SFXClipData> sfxDictionary;

    private Dictionary<ESFXType, float> sfxPlayTime;
    private float sfxPlayCool = 0.07f;

    private BGMClipData currentBGMData;
    private float masterVolume = 1f;
    private float bgmVolume = 1f;
    private float sfxVolume = 1f;

    public float GetBGMVolume() => bgmVolume;
    public float GetSFXVolume() => sfxVolume;
    
    private static AudioManager instance;

    #region 싱글톤
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AudioManager>();

                if (instance == null)
                {
                    GameObject obj = new GameObject(typeof(AudioManager).Name);
                    instance = obj.AddComponent<AudioManager>();
                }
            }
            return instance;
        }
    }
    #endregion
    private void Awake()
    {
        if (instance == null)
        {
            instance = this as AudioManager;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (instance != this)
            {
                Destroy(gameObject);
            }
        }
        CreateAudioSource();
        InitDictionary();
    }

    private void CreateAudioSource()
    {
        if (bgmSource == null)
        {
            GameObject bgmObj = new GameObject("BGM Source");
            bgmObj.transform.SetParent(transform);

            bgmSource = bgmObj.AddComponent<AudioSource>();
            bgmSource.loop = true;
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFX Source");
            sfxObj.transform.SetParent(transform);

            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
        }
    }

    private void InitDictionary()
    {
        bgmDictionary = new Dictionary<EBGMType, BGMClipData>();
        sfxDictionary = new Dictionary<ESFXType, SFXClipData>();
        sfxPlayTime = new Dictionary<ESFXType, float>();

        if (audioData == null) return;

        for (int i = 0; i < audioData.bgmClips.Length; i++)
        {
            if (audioData.bgmClips[i] == null) continue;
            if (audioData.bgmClips[i].clip == null) continue;

            if (!bgmDictionary.ContainsKey(audioData.bgmClips[i].type))
            {
                bgmDictionary.Add(audioData.bgmClips[i].type, audioData.bgmClips[i]);
            }
        }

        for (int i = 0; i < audioData.sfxClips.Length; i++)
        {
            if (audioData.sfxClips[i] == null) continue;
            if (audioData.sfxClips[i].clip == null) continue;

            if (!sfxDictionary.ContainsKey(audioData.sfxClips[i].type))
            {
                sfxDictionary.Add(audioData.sfxClips[i].type, audioData.sfxClips[i]);
            }
        }
    }

    // BGM 재생
    public void PlayBGM(EBGMType type)
    {
        if (!bgmDictionary.ContainsKey(type)) return;

        BGMClipData data = bgmDictionary[type];

        if (bgmSource.clip == data.clip) return;

        currentBGMData = data;
        bgmSource.clip = data.clip;
        bgmSource.volume = data.volume * bgmVolume * masterVolume;

        bgmSource.Play();
    }
    // BGM 중단
    public void StopBGM()
    {
        bgmSource.Stop();
        bgmSource.clip = null;
        currentBGMData = null;
    }
    // BGM 일시 정지
    public void PauseBGM()
    {
        bgmSource.Pause();
    }
    // BGM 재개
    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }

    // SFX 재생
    public void PlaySFX(ESFXType type)
    {
        if (!sfxDictionary.ContainsKey(type)) return;

        if (sfxPlayTime.ContainsKey(type))
        {
            if (Time.time < sfxPlayTime[type] + sfxPlayCool) return;

            sfxPlayTime[type] = Time.time;
        }
        else
        {
            sfxPlayTime.Add(type, Time.time);
        }
        SFXClipData data = sfxDictionary[type];

        float volume = data.volume * sfxVolume * masterVolume;
        sfxSource.PlayOneShot(data.clip, volume);
    }

    // UI : BGM 볼륨 조절
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateBGMVolume();
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        UpdateBGMVolume();
    }

    public void UpdateBGMVolume()
    {
        if (bgmSource == null) return;
        if (currentBGMData == null)
        {
            bgmSource.volume = bgmVolume * masterVolume;
            return;
        }
        bgmSource.volume = currentBGMData.volume * bgmVolume * masterVolume;
    }

    // UI : SFX 볼륨 조절
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
}
