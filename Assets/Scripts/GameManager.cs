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

public class GameManager : SingletonBehaviour<GameManager> {

    [Header("Editor")]
    public bool spawnAtSceneViewCamera = true;

    [Header("Player")]
    public FPSPlayerController playerPrefab = null;
    public FPSPlayerController Player { get { return m_player; } }


    [Header("Stages")]
    public int firstStageIndex = 0;
    public List<GameStage> gameStages = new List<GameStage>();

    private FPSPlayerController m_player = null;
    private int m_currentStageIndex = -1;
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

        //Start first stage
        AdvanceToStage(firstStageIndex);
        if (m_currentStageIndex >= 0 && m_currentStageIndex < gameStages.Count)
        {
            gameStages[m_currentStageIndex].RespawnPlayer(Player);
        }
    }

    protected void Update()
    {
        if (Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                if (Player) Player.GetComponent<Health>().TakeDamage(new DamagePacket(null, Vector3.zero, true));
            }
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                if (Player) Player.GetComponent<Health>().Heal();
            }
        }

        if (m_currentStageIndex >= 0 && m_currentStageIndex < gameStages.Count)
        {
            if(gameStages[m_currentStageIndex].IsStageFinished())
            {
                if(!AdvanceToStage(m_currentStageIndex + 1))
                {
                    StartCoroutine(EndingHack());
                }
            }
        }
    }

    public void OnPlayerKilled()
    {
        if (Player && Player.m_fpsHUD)
        {
            Player.m_fpsHUD.OnDeath();
        }
        RestartCurrentStage();
    }

    public void OnEnemyKilled(Health enemy)
    {
        
    }

    IEnumerator EndingHack()
    {
        if(Player.m_fpsHUD)
        {
            Player.m_fpsHUD.TryShowPrompt(new PromptSetup("You killed all the things. Thanks for playing!", 0, 10.0f));
        }
        yield return new WaitForSecondsRealtime(1.0f);
        for(int i = 5; i > 0; i--)
        {
            if (Player.m_fpsHUD)
            {
                Player.m_fpsHUD.TryShowPrompt(new PromptSetup("Restarting in " + i + "...", 0, 1.0f));
            }
            yield return new WaitForSecondsRealtime(1.0f);
        }
        AdvanceToStage(0, true);
    }

    public bool AdvanceToStage(int stageIndex, bool forceRespawn = false)
    {
        if (m_currentStageIndex >= 0 && m_currentStageIndex < gameStages.Count)
        {
            gameStages[m_currentStageIndex].OnStageEnded();
        }
        m_currentStageIndex = stageIndex;
        if (m_currentStageIndex >= 0 && m_currentStageIndex < gameStages.Count)
        {
            gameStages[m_currentStageIndex].OnStageBegan();
            if(forceRespawn)
            {
                gameStages[m_currentStageIndex].RespawnPlayer(Player);
            }
            return true;
        }
        return false;
    }

    public void RestartCurrentStage()
    {
        if (m_currentStageIndex >= 0 && m_currentStageIndex < gameStages.Count)
        {
            gameStages[m_currentStageIndex].OnStageEnded();
            gameStages[m_currentStageIndex].OnStageBegan();
            gameStages[m_currentStageIndex].RespawnPlayer(Player);
        }
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
}
