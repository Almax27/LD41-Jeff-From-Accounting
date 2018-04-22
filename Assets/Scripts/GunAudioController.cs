using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GunAudio
{
    public List<AudioClip> m_clips = new List<AudioClip>();
    public float m_pitch = 1.0f;
    public float m_pitchVariance = 0.0f;
    public float m_volume = 1.0f;
    public float m_volumeVariance = 0.0f;
}

[RequireComponent(typeof(AudioSource))]
public class GunAudioController : MonoBehaviour {

    public GunAudio ReloadStart;
    public GunAudio ReloadKeyPress;
    public GunAudio ReloadEnd;
    public GunAudio Fire;
    public GunAudio DryFire;

    AudioSource m_audioSource = null;

    private void Awake()
    {
        m_audioSource = GetComponent<AudioSource>();
        Debug.Assert(m_audioSource);
    }

    void PlayGunAudio(GunAudio gunAudio)
    {
        if(m_audioSource && gunAudio != null && gunAudio.m_clips.Count > 0)
        {
            m_audioSource.clip = gunAudio.m_clips[Random.Range(0, gunAudio.m_clips.Count)];
            m_audioSource.pitch = gunAudio.m_pitch + Random.Range(-gunAudio.m_pitchVariance, gunAudio.m_pitchVariance);
            m_audioSource.volume = gunAudio.m_volume + Random.Range(-gunAudio.m_volumeVariance, gunAudio.m_volumeVariance);
            m_audioSource.Play();
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
