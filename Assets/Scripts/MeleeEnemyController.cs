using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MeleeEnemyController : MonoBehaviour {

    public float m_aggroRange = 10;

    Transform m_target = null;
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
            FPSPlayerController player = GameManager.Instance.Player;
            if(player && Vector3.Distance(transform.position, player.transform.position) < m_aggroRange)
            {
                m_target = player.transform;
            }
        }
        if(m_target && m_agent)
        {
            m_agent.SetDestination(m_target.position);
        }
	}

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(this.transform.position, m_aggroRange);
    }
}
