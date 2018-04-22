using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MeleeEnemyController : MonoBehaviour {

    GameObject m_target = null;
    NavMeshAgent m_agent = null;

    // Use this for initialization
    void Awake () {
        m_agent = GetComponent<NavMeshAgent>();

    }
	
	// Update is called once per frame
	void Update ()
    {
	    if(m_target == null)
        {
            m_target = GameObject.FindGameObjectWithTag("Player");
        }
        if(m_target && m_agent)
        {
            m_agent.SetDestination(m_target.transform.position);
        }
	}
}
