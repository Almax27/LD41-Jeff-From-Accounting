using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameGenerator : MonoBehaviour {

    public List<string> names = new List<string>();

    static int s_seed = -1;

    private void Start()
    {
        if(s_seed < 0)
        {
            s_seed = System.DateTime.Now.Second;
        }
        Health health = GetComponent<Health>();
        if(health && names.Count > 0)
        {
            int index = ++s_seed % names.Count;
            health.SetHealth(names[index]);
        }
    }
}
