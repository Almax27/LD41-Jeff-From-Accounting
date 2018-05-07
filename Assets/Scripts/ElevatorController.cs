using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class ElevatorController : MonoBehaviour {

    public PlayableDirector m_director = null;

    public List<MusicSetup> musicOnEnter = new List<MusicSetup>();
    public List<MusicSetup> musicOnExit = new List<MusicSetup>();

    public float m_openTimeOffset = 8.0f;

    bool m_triggered = false;
    bool m_playerInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if (m_director && !m_triggered)
            {
                m_director.time = 0;
                m_director.Play();
                m_triggered = true;
            }
            m_playerInside = true;
            FAFAudio.Instance.TryPlayMusic(musicOnEnter);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" && m_playerInside)
        {
            FAFAudio.Instance.TryPlayMusic(musicOnExit);
        }
    }

    public void Reset()
    {
        m_triggered = false;
        m_playerInside = false;
        if (m_director)
        {
            m_director.time = 0;
            m_director.Evaluate();
        }
    }

    public void OpenExit()
    {
        if (m_director)
        {
            m_director.time = m_openTimeOffset;
            m_director.Play();
            m_triggered = true;
        }
    }
}
