using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformActivation : MonoBehaviour
{

    public bool ActiveDefault = true;
    [Header("Platform")]
    public bool ActiveOnMobile = true;
    public bool ActiveOnConsole = true;

    // Use this for initialization
    void Awake ()
    {
		if( !ActiveOnMobile && Application.isMobilePlatform ||
            !ActiveOnConsole && Application.isConsolePlatform)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
	}
}
