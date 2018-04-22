using System.Collections;
using System.Collections.Generic;
using UnityEditor;

#if UNITY_EDITOR
using UnityEngine;
#endif

public enum GameState
{
    Idle,
    Combat
}


public class GameManager : MonoBehaviour {

    [Header("Editor")]
    public bool spawnAtSceneViewCamera = true;

    [Header("Player")]
    public FPSPlayerController playerPrefab = null;

    [Header("Music")]
    public MusicSetup idleMusic = null;
    public MusicSetup combatBuildupMusic = null;
    public MusicSetup combatMusic = null;

    private GameState m_state = GameState.Idle;

    // Use this for initialization
    void Start () {
        if(playerPrefab)
        {
            GameObject gobj = Instantiate<GameObject>(playerPrefab.gameObject);
#if UNITY_EDITOR
            if(spawnAtSceneViewCamera && SceneView.sceneViews.Count > 0)
            {
                SceneView Scene = SceneView.sceneViews[0] as SceneView;
                if(Scene)
                {
                    gobj.transform.position = Scene.pivot;
                    Vector3 euler = Scene.lastSceneViewRotation.eulerAngles;
                    gobj.transform.rotation = Quaternion.Euler(0, euler.y, 0);
                }
            }
#endif
        }
        SetGameState(GameState.Idle, true);
        StartCoroutine(Hack());
    }

    IEnumerator Hack()
    {
        yield return new WaitForSeconds(10.0f);
        SetGameState(GameState.Combat);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void SetGameState(GameState newState, bool force = false)
    {
        if (!force && newState == m_state) return;
        m_state = newState;
        switch (m_state)
        {
            case GameState.Idle:
                FAFAudio.Instance.TryPlayMusic(idleMusic);
                break;
            case GameState.Combat:
                FAFAudio.Instance.TryPlayMusic(combatBuildupMusic);
                FAFAudio.Instance.TryPlayMusic(combatMusic, true);
                break;
            default:
                break;
        }
    }
}
