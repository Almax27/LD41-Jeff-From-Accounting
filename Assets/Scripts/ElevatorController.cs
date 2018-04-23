using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class ElevatorController : MonoBehaviour {

    public PlayableDirector m_director = null;

    bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if(!triggered && other.tag == "Player" && m_director)
        {
            m_director.Play();
            triggered = true;
        }
    }
}
