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
    private bool m_isPaused = false;

    // Use this for initialization
    protected void Awake () {

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

    protected override void Start()
    {
        base.Start();
        SetGameState(GameState.Idle, true);

        //START MESSAGE HACK
        if (Player.m_fpsHUD)
        {
            Player.m_fpsHUD.TryShowPrompt(new PromptSetup("Jeff from Accounting\nMade for LudumDare 41\nBy Greg Lee, Dale Smith and Aaron Baumbach", 0, 10.0f));
        }
    }

    public void OnEnemyKilled(Health enemy)
    {
        bool allEnemiesDead = true;
        foreach(Health health in FindObjectsOfType<Health>())
        {
            if(health.IsAlive() && health.tag != "Player")
            {
                allEnemiesDead = false;
                break;
            }
        }
        if (allEnemiesDead)
        {
            StartCoroutine(EndingHack());
        }
    }

    IEnumerator EndingHack()
    {
        if(Player.m_fpsHUD)
        {
            Player.m_fpsHUD.TryShowPrompt(new PromptSetup("You killed all the things. Thanks for playing!", 0, 10.0f));
        }
        yield return new WaitForSecondsRealtime(10.0f);
        for(int i = 5; i > 0; i--)
        {
            if (Player.m_fpsHUD)
            {
                Player.m_fpsHUD.TryShowPrompt(new PromptSetup("Restarting in " + i + "...", 0, 1.0f));
            }
            yield return new WaitForSecondsRealtime(1.0f);
        }
        ReloadLevel();
    }

    public void OnDoorKilled()
    {
        SetGameState(GameState.Combat);
    }

    public void SetPaused(bool pause)
    {
        if (m_isPaused == pause) return;
        m_isPaused = pause;
        if (pause)
        {
            Time.timeScale = 0;
            if (Player && Player.m_fpsHUD)
            {
                Player.m_fpsHUD.OnPaused();
            }
        }
        else
        {
            Time.timeScale = 1;
            if (Player && Player.m_fpsHUD)
            {
                Player.m_fpsHUD.OnUnpaused();
            }
        }
    }

    public void ReloadLevel()
    {
        for (int i = 1; i < 5; i++)
        {
            if (i == 1)
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
