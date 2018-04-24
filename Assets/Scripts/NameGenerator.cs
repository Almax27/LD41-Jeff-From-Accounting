using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameGenerator : MonoBehaviour {

    public List<string> names = new List<string>();

    private void Start()
    {
        Health health = GetComponent<Health>();
        if(health && names.Count > 0)
        {
            string n = names[Random.Range(0, names.Count - 1)];
            health.SetHealth(n);
        }
    }
}
