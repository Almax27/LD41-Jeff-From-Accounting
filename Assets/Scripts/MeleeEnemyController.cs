using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MeleeEnemyController : MonoBehaviour {

    public float m_aggroRange = 10;
    public float m_attackRange = 3.0f;
    public float m_attackRate = 1.0f;

    Transform m_target = null;
    NavMeshAgent m_agent = null;

    float attackTick = 0;

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
        if(m_target)
        {
            attackTick += Time.deltaTime;
            if (attackTick > m_attackRate && Vector3.Distance(transform.position, m_target.position) < m_attackRange)
            {
                Health health = m_target.GetComponent<Health>();
                if(health)
                {
                    DamagePacket dmg = new DamagePacket();
                    dmg.forceLetterMatch = true;
                    health.TakeDamage(dmg);
                    attackTick = 0;
                }
            }
        }
	}

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(this.transform.position, m_aggroRange);
    }
}
