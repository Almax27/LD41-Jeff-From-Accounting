using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStage : MonoBehaviour {

    public Transform respawnPoint = null;
    public GameObject enemiesRoot = null;
    public List<MusicSetup> stageMusic = new List<MusicSetup>();

    public bool IsStageActive { get { return m_isStageActive; } }

    bool m_isStageActive = false;
    GameObject m_enemiesRootInstance = null;
    List<Health> m_activeEnemies = new List<Health>();

    protected virtual void Awake()
    {
        if (enemiesRoot)
        {
            enemiesRoot.SetActive(false);
        }
    }

    public virtual bool IsStageFinished()
    {
        foreach(Health enemy in m_activeEnemies)
        {
            if (enemy && enemy.isActiveAndEnabled)
                return false;
        }
        return true;
    }

    public virtual void OnStageBegan()
    {
        Debug.Log("OnStageBegan: " + gameObject.name);

        m_isStageActive = true;

        m_activeEnemies.Clear();
        if (m_enemiesRootInstance)
        {
            Destroy(m_enemiesRootInstance);
        }
        if(enemiesRoot)
        { 
            m_enemiesRootInstance = new GameObject("ActiveEnemies");
            m_enemiesRootInstance.transform.parent = this.transform;
            foreach (Transform t in enemiesRoot.transform)
            {
                GameObject e = Instantiate<GameObject>(t.gameObject);
                e.transform.parent = m_enemiesRootInstance.transform;
                foreach (Health enemy in e.GetComponentsInChildren<Health>())
                {
                    if (enemy.isActiveAndEnabled)
                    {
                        m_activeEnemies.Add(enemy);
                    }
                }
            }
        }
        for(int i = 0; i < stageMusic.Count; i++)
        {
            FAFAudio.Instance.TryPlayMusic(stageMusic[i], i > 0);
        }
    }

    public virtual void OnStageEnded()
    {
        m_isStageActive = false;

        Debug.Log("OnStageEnded: " + gameObject.name);
        Destroy(m_enemiesRootInstance);
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
