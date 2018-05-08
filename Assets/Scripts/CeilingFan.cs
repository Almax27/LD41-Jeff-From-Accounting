using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CeilingFan : MonoBehaviour {

    public Transform rotationRoot = null;
    public float rotationSpeed = 180.0f;
	
	void Update ()
    {
	    if(rotationRoot)
        {
            rotationRoot.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0));
        }
	}
}
