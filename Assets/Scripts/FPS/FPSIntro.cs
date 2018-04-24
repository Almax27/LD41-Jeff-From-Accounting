using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSIntro : MonoBehaviour {

    public bool m_autoPlay = true;

    public Animation m_introAnimation = null;

    private void Start()
    {
        if (m_introAnimation == null)
        {
            m_introAnimation = GetComponent<Animation>();
        }
        if (m_introAnimation && m_autoPlay && (!Application.isEditor || !GameManager.Instance.spawnAtSceneViewCamera))
        {
            m_introAnimation.Play();
            OnIntroStart();
        }
        else
        {
            OnIntroFinished();
        }
    }

    public void OnIntroStart()
    {
        FPSPlayerController player = GameManager.Instance.Player;
        if (player)
        {
            player.m_isInputEnabled = false;
            player.m_gunController.gameObject.SetActive(false);
        }
    }

    public void OnIntroFinished()
    {
        FPSPlayerController player = GameManager.Instance.Player;
        if (player)
        {
            player.m_isInputEnabled = true;
            player.m_gunController.gameObject.SetActive(true);
            player.m_gunController.SetGunUp(true);
        }
    }
}
