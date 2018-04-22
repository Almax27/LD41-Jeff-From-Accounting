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

public class GunAudioController : MonoBehaviour {

    [Header("Setup")]
    public GunAudio ReloadStart;
    public GunAudio ReloadKeyPress;
    public GunAudio ReloadEnd;
    public GunAudio Fire;
    public GunAudio DryFire;

    [Header("Sources")]
    public AudioSource m_muzzleSource = null;
    public AudioSource m_reloadSource = null;

    void PlayGunAudio(AudioSource source, GunAudio gunAudio)
    {
        if(source && gunAudio != null && gunAudio.m_clips.Count > 0)
        {
            source.clip = gunAudio.m_clips[Random.Range(0, gunAudio.m_clips.Count)];
            source.pitch = gunAudio.m_pitch + Random.Range(-gunAudio.m_pitchVariance, gunAudio.m_pitchVariance);
            source.volume = gunAudio.m_volume + Random.Range(-gunAudio.m_volumeVariance, gunAudio.m_volumeVariance);
            source.Play();
        }
    }

    public void OnReloadStart()
    {
        PlayGunAudio(m_reloadSource, ReloadStart);
    }

    public void OnReloadKeyPress()
    {
        PlayGunAudio(m_reloadSource, ReloadKeyPress);
    }

    public void OnReloadEnd()
    {
        PlayGunAudio(m_reloadSource, ReloadEnd);
    }

    public void OnFire()
    {
        PlayGunAudio(m_muzzleSource, Fire);
    }

    public void OnDryFire()
    {
        PlayGunAudio(m_muzzleSource, DryFire);
    }
}
