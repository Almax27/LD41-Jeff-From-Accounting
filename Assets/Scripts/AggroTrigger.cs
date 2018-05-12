using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class AggroTrigger : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            foreach(Transform child in transform)
            {
                EnemyController enemy = child.GetComponent<EnemyController>();
                if (enemy)
                {
                    enemy.SetTarget(other.transform);
                }
            }
        }
    }
}
