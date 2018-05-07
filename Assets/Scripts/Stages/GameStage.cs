using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStage : MonoBehaviour {

    public Transform respawnPoint = null;
    public GameObject enemiesRoot = null;
    public List<MusicSetup> stageMusic = new List<MusicSetup>();

    GameObject m_activeEnemies = null;

    public void Start()
    {
        if (enemiesRoot)
        {
            enemiesRoot.SetActive(false);
        }
    }

    public virtual bool IsStageFinished()
    {
        return m_activeEnemies == null || m_activeEnemies.transform.childCount <= 0;
    }

    public virtual void OnStageBegan()
    {
        Debug.Log("OnStageBegan: " + gameObject.name);
        if (m_activeEnemies)
        {
            Destroy(m_activeEnemies);
        }
        if(enemiesRoot)
        { 
            m_activeEnemies = new GameObject("ActiveEnemies");
            m_activeEnemies.transform.parent = this.transform;
            foreach (Transform t in enemiesRoot.transform)
            {
                GameObject e = Instantiate<GameObject>(t.gameObject);
                e.transform.parent = m_activeEnemies.transform;
            }
        }
        for(int i = 0; i < stageMusic.Count; i++)
        {
            FAFAudio.Instance.TryPlayMusic(stageMusic[i], i > 0);
        }
    }

    public virtual void OnStageEnded()
    {
        Debug.Log("OnStageEnded: " + gameObject.name);
        Destroy(m_activeEnemies);
    }

    public virtual void RespawnPlayer(FPSPlayerController player)
    {
        Debug.Log("RespawnPlayer: " + gameObject.name);
        if (player && respawnPoint)
        {
            player.gameObject.SetActive(false); //diable for teleport so we skip trigger exits
            player.transform.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);
            player.OnRespawn();
            player.gameObject.SetActive(true);
        }
    }

}
