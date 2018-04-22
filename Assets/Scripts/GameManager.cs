using System.Collections;
using System.Collections.Generic;
using UnityEditor;

#if UNITY_EDITOR
using UnityEngine;
#endif

public class GameManager : MonoBehaviour {

    [Header("Editor")]
    public bool spawnAtSceneViewCamera = true;

    [Header("Player")]
    public FPSPlayerController playerPrefab = null;

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

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
