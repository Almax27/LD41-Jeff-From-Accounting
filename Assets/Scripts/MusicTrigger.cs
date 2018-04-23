using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTrigger : MonoBehaviour {

    public MusicSetup musicOnEnter = null;
    public MusicSetup musicOnExit = null;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            FAFAudio.Instance.TryPlayMusic(musicOnEnter);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            FAFAudio.Instance.TryPlayMusic(musicOnExit);
        }
    }
}
