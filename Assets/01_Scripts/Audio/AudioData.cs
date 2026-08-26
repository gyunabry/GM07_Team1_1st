using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData", menuName = "AudioData / AudioData")]
public class AudioData : ScriptableObject
{
    [Header("BGM List")]
    public BGMClipData[] bgmClips;
    [Header("SFX List")]
    public SFXClipData[] sfxClips;
}

[Serializable]
public class BGMClipData
{
    public EBGMType type;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;
}

[Serializable]
public class SFXClipData
{
    public ESFXType type;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;
}
