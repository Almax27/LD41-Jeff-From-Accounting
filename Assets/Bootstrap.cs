using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    private void Start()
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
}
