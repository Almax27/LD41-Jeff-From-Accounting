using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTrigger : MonoBehaviour {

    public List<MusicSetup> musicOnEnter = new List<MusicSetup>();
    public List<MusicSetup> musicOnExit = new List<MusicSetup>();

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
