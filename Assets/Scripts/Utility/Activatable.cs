using UnityEngine;
using System.Collections;

[System.Serializable]
public class Activatable
{
    public GameObject gobj;
    public MonoBehaviour script;
    public bool active;
}

[System.Serializable]
public class ActivatableGroup
{
    public Activatable[] activatables = new Activatable[0];

    public void Process()
    {
        for(int i = 0; i < activatables.Length; i++)
        {
            Activatable activatable = activatables[i];

            if(activatable.gobj)
                activatable.gobj.SetActive(activatable.active);

            if(activatable.script)
                activatable.script.enabled = activatable.active;
        }
    }
}
