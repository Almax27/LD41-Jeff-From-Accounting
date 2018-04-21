using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AudioRepeater : MonoBehaviour 
{

    #region public variables
   
    public float alarmSoundDelay = 0.5f;

    #endregion

    #region protected variables

    public float alarmSoundTick = 0;

    #endregion

    #region monobehaviour methods

    void Update()
    {
        alarmSoundTick += Time.deltaTime;
        if(alarmSoundTick > alarmSoundDelay)
        {
            alarmSoundTick -= alarmSoundDelay;
            this.GetComponent<AudioSource>().Play();
        }
    }

    #endregion
}
