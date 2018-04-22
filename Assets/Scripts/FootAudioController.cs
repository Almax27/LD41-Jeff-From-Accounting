using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FootAudio
{
    public GroundType m_groundType = GroundType.Default;
    public List<AudioClip> m_footsteps = new List<AudioClip>();
    public AudioClip m_jump = null;
    public AudioClip m_land = null;
    public float m_pitchVariance = 0.0f;
}

[RequireComponent(typeof(AudioSource))]
public class FootAudioController : MonoBehaviour {

    public List<FootAudio> m_setup = new List<FootAudio>();

    protected AudioSource m_audioSource = null;

    void Awake()
    {
        m_audioSource = GetComponent<AudioSource>();
        Debug.Assert(m_audioSource);
    }

    public void OnStep(GroundType groundType = GroundType.Default)
    {
        FootAudio footAudio = m_setup.Find(f => f.m_groundType == groundType);
        if(footAudio != null && footAudio.m_footsteps.Count > 0)
        {
            TryPlay(footAudio.m_footsteps[Random.Range(0, footAudio.m_footsteps.Count)], footAudio.m_pitchVariance);
        }
    }

    public void OnJump(GroundType groundType = GroundType.Default)
    {
        FootAudio footAudio = m_setup.Find(f => f.m_groundType == groundType);
        if (footAudio != null)
        {
            TryPlay(footAudio.m_jump, footAudio.m_pitchVariance);
        }
    }

    public void OnLand(GroundType groundType = GroundType.Default)
    {
        FootAudio footAudio = m_setup.Find(f => f.m_groundType == groundType);
        if (footAudio != null)
        {
            TryPlay(footAudio.m_land, footAudio.m_pitchVariance);
        }
    }

    public void TryPlay(AudioClip clip, float pitchVariance)
    {
        if (m_audioSource)
        {
            m_audioSource.clip = clip;
            m_audioSource.pitch = 1.0f + Random.Range(-pitchVariance, pitchVariance);
            m_audioSource.Play();
        }
    }
}
