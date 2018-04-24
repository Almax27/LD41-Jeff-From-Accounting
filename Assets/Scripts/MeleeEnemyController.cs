using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MeleeEnemyController : MonoBehaviour {

    public float m_aggroRange = 10;
    public float m_attackRange = 3.0f;
    public float m_attackRate = 1.0f;

    public AnimationCurve m_shakeCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.25f, 1f),
                                                            new Keyframe(0.5f, 0f), new Keyframe(0.75f, -1f),
                                                            new Keyframe(1f, 0f)); // sin curve for head bob
    public Transform transformToShake = null;
    public float shakeMag = 0.05f;

    public GameObject m_eyes = null;
    Vector3 originalShakePos = Vector3.zero;

    float m_speedBoostTimer = 0;
    float m_baseSpeed;

    Transform m_target = null;
    NavMeshAgent m_agent = null;
    Health m_health = null;

    float attackTick = 0;

    public void SetTarget(Transform target)
    {
        m_target = target;
        m_speedBoostTimer = 3.0f;
    }

    // Use this for initialization
    void Awake () {
        m_agent = GetComponent<NavMeshAgent>();
        m_health = GetComponent<Health>();
        if (transformToShake)
        {
            originalShakePos = transformToShake.localPosition;
        }
        attackTick = m_attackRate;
        m_shakeCurve.postWrapMode = WrapMode.Loop;
        if (m_agent)
        {
            m_baseSpeed = m_agent.speed;
        }
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
            m_speedBoostTimer = Mathf.Max(0, m_speedBoostTimer - Time.deltaTime);
            float speed = m_baseSpeed * (m_speedBoostTimer > 0 ? 2.0f : 1.0f);
            m_agent.speed = speed;
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
            if (transformToShake && attackTick * 3.0f < m_attackRate)
            {
                float shake = m_shakeCurve.Evaluate((attackTick / m_attackRate) * 10.0f);
                transformToShake.localPosition = originalShakePos + new Vector3(0.7f, 1.2f, 0.5f) * shake * shakeMag;
            }
        }
        if (m_eyes)
        {
            m_eyes.SetActive(m_target != null);
        }
        if (m_health)
        {
            m_health.m_ignoreRaycat = m_target == null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(this.transform.position, m_aggroRange);
    }
}
