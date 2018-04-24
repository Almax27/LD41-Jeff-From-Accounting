using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoadAttribute]
public static class GameManagerPlayModeStateChanged
{
    // register an event handler when the class is initialized
    static GameManagerPlayModeStateChanged()
    {
        EditorApplication.playModeStateChanged += LogPlayModeState;
    }

    static string Vector3ToString(Vector3 v)
    { // change 0.00 to 0.0000 or any other precision you desire, i am saving space by using only 2 digits
        return string.Format("{0:0.00},{1:0.00},{2:0.00}", v.x, v.y, v.z);
    }

    static Vector3 Vector3FromString(string s)
    {
        string[] parts = s.Split(',');
        return new Vector3(
            float.Parse(parts[0]),
            float.Parse(parts[1]),
            float.Parse(parts[2]));
    }

    private static void LogPlayModeState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode && SceneView.sceneViews.Count > 0)
        {
            SceneView Scene = SceneView.sceneViews[0] as SceneView;
            if (Scene && Scene.camera)
            {
                EditorPrefs.SetString("SpawnPos", Vector3ToString(Scene.camera.transform.position));
                Vector3 euler = Scene.camera.transform.rotation.eulerAngles;
                EditorPrefs.SetString("SpawnRot", Vector3ToString(euler));
            }
        }
        else if (state == PlayModeStateChange.EnteredPlayMode)
        {
            GameManager[] gms = GameObject.FindObjectsOfType<GameManager>();
            foreach(GameManager gm in gms)
            {
                if(gm.Player && gm.spawnAtSceneViewCamera)
                {
                    gm.Player.transform.position = Vector3FromString(EditorPrefs.GetString("SpawnPos"));
                    Vector3 euler = Vector3FromString(EditorPrefs.GetString("SpawnRot"));
                    gm.Player.transform.rotation = Quaternion.Euler(0, euler.y, 0);
                }
            }
        }
    }
}
#endif

public enum GameState
{
    Idle,
    Combat
}


public class GameManager : SingletonBehaviour<GameManager> {

    [Header("Editor")]
    public bool spawnAtSceneViewCamera = true;

    [Header("Player")]
    public FPSPlayerController playerPrefab = null;
    public FPSPlayerController Player { get { return m_player; } }

    [Header("Music")]
    public MusicSetup idleMusic = null;
    public MusicSetup combatBuildupMusic = null;
    public MusicSetup combatMusic = null;

    private GameState m_state = GameState.Idle;
    private FPSPlayerController m_player = null;

    // Use this for initialization
    protected override void Awake () {

        m_player = FindObjectOfType<FPSPlayerController>();
        if(!m_player && playerPrefab)
        {
            GameObject gobj = Instantiate<GameObject>(playerPrefab.gameObject);
            if(gobj)
            {
                m_player = gobj.GetComponent<FPSPlayerController>();
            }
        }
    }

    private void Start()
    {
        SetGameState(GameState.Idle, true);
    }

    public void OnDoorKilled()
    {
        SetGameState(GameState.Combat);
    }

    public void ReloadLevel()
    {
        for (int i = 1; i < 5; i++)
        {
            if (i == 0)
            {
                SceneManager.LoadScene(i, LoadSceneMode.Single);
            }
            else
            {
                SceneManager.LoadScene(i, LoadSceneMode.Additive);
            }
        }
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
