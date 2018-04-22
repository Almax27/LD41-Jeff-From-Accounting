using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class GunAudio
{
    public List<AudioClip> m_clips = new List<AudioClip>();
    public float m_pitch = 1.0f;
    public float m_pitchVariance = 0.0f;
    public float m_volume = 1.0f;
    public float m_volumeVariance = 0.0f;
    public AudioMixerGroup m_mixerGroup = null;
}

public class GunAudioController : MonoBehaviour {

    [Header("Setup")]
    public GunAudio ReloadStart;
    public GunAudio ReloadKeyPress;
    public GunAudio ReloadEnd;
    public GunAudio Fire;
    public GunAudio DryFire;

    void PlayGunAudio(GunAudio gunAudio)
    {
        if(gunAudio != null && gunAudio.m_clips.Count > 0)
        {
            AudioClip clip = gunAudio.m_clips[Random.Range(0, gunAudio.m_clips.Count)];
            float volume = gunAudio.m_volume + Random.Range(-gunAudio.m_volumeVariance, gunAudio.m_volumeVariance);
            float pitch = gunAudio.m_pitch + Random.Range(-gunAudio.m_pitchVariance, gunAudio.m_pitchVariance);
            FAFAudio.Instance.Play(clip, transform.position, volume, pitch, gunAudio.m_mixerGroup);
        }
    }

    public void OnReloadStart()
    {
        PlayGunAudio(ReloadStart);
    }

    public void OnReloadKeyPress()
    {
        PlayGunAudio(ReloadKeyPress);
    }

    public void OnReloadEnd()
    {
        PlayGunAudio(ReloadEnd);
    }

    public void OnFire()
    {
        PlayGunAudio(Fire);
    }

    public void OnDryFire()
    {
        PlayGunAudio(DryFire);
    }
}
