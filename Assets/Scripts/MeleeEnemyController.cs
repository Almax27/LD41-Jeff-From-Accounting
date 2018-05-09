using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum MeleeEnemyState
{
    Moving,
    Knockback
}


[RequireComponent(typeof(NavMeshAgent))]
public class MeleeEnemyController : MonoBehaviour {

    public float m_aggroRange = 10;
    public float m_attackRange = 3.0f;
    public float m_attackRate = 1.0f;

    public float m_knockbacfkSpeed = 5.0f;
    public float m_knockbackfDuration = 0.1f;
    public float m_knockbackSftunDuration = 0.25f;

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

    MeleeEnemyState m_state = MeleeEnemyState.Moving;

    float m_attackTick = 0.0f;
    float m_knockbackTick = 0.0f;
    Vector3 m_knockbackTargetPosition = Vector3.zero;
    Vector3 m_knockbackVelocity = Vector3.zero;

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
        m_attackTick = m_attackRate;
        m_shakeCurve.postWrapMode = WrapMode.Loop;
        if (m_agent)
        {
            m_baseSpeed = m_agent.speed;
        }
    }

    private void OnEnable()
    {
        m_state = MeleeEnemyState.Moving;
        m_attackTick = m_attackRate;
        m_knockbackTick = float.MaxValue;
        m_knockbackTargetPosition = transform.position;
    }

    // Update is called once per frame
    void Update ()
    {
        //Lazy find target
        if (m_target == null)
        {
            FPSPlayerController player = GameManager.Instance.Player;
            if (player && Vector3.Distance(transform.position, player.transform.position) < m_aggroRange)
            {
                m_target = player.transform;
            }
        }

        //Update state
        switch (m_state)
        {
            case MeleeEnemyState.Moving:
                UpdateMoving();
                break;
            case MeleeEnemyState.Knockback:
                UpdateKnockback();
                break;
            default:
                break;
        }

        //Update misc
        if (m_eyes)
        {
            m_eyes.SetActive(m_target != null);
        }
        if (m_health)
        {
            m_health.m_ignoreRaycat = m_target == null;
        }
    }

    void UpdateMoving()
    {
        if(m_agent)
        {
            m_agent.isStopped = false;

            if (m_target)
            {
                m_speedBoostTimer = Mathf.Max(0, m_speedBoostTimer - Time.deltaTime);
                float speed = m_baseSpeed * (m_speedBoostTimer > 0 ? 2.0f : 1.0f);
                m_agent.speed = speed;
                m_agent.SetDestination(m_target.position);
            }
        }
        
        if (m_target)
        {
            if (Vector3.Distance(transform.position, m_target.position) < m_attackRange)
            {
                m_attackTick += Time.deltaTime;
                if (m_attackTick > m_attackRate)
                {
                    Health health = m_target.GetComponent<Health>();
                    if (health)
                    {
                        Vector3 hitNormal = transform.position - m_target.position;
                        hitNormal.Normalize();
                        health.TakeDamage(new DamagePacket(this.gameObject, hitNormal, true));
                        m_attackTick = 0;
                    }
                }
            }
            if (transformToShake && m_attackTick * 3.0f < m_attackRate)
            {
                float shake = m_shakeCurve.Evaluate((m_attackTick / m_attackRate) * 10.0f);
                transformToShake.localPosition = originalShakePos + new Vector3(0.7f, 1.2f, 0.5f) * shake * shakeMag;
            }
        }
    }

    void UpdateKnockback()
    {
        if (m_agent)
        {
            m_agent.isStopped = true;
        }
        m_knockbackTick += Time.deltaTime;
        if (m_knockbackTick < m_knockbackfDuration)
        {
            transform.position = Vector3.SmoothDamp(transform.position, m_knockbackTargetPosition, ref m_knockbackVelocity, m_knockbackfDuration, m_knockbacfkSpeed);
            //transform.position = Vector3.MoveTowards(transform.position, m_knockbackTargetPosition, m_knockbackSpeed * Time.deltaTime);
        }
        else if(m_knockbackTick < m_knockbackSftunDuration)
        {
            //stunned
        }
        else
        {
            m_state = MeleeEnemyState.Moving;
        }
    }

    void OnDamage(DamagePacket packet)
    {
        if (m_knockbackfDuration > 0)
        {
            m_state = MeleeEnemyState.Knockback;
            m_knockbackTargetPosition = transform.position - packet.hitNormal * (m_knockbacfkSpeed / m_knockbackfDuration);
            NavMeshHit hit;
            if (NavMesh.Raycast(transform.position, m_knockbackTargetPosition, out hit, NavMesh.AllAreas))
            {
                m_knockbackTargetPosition = hit.position;
            }
            m_knockbackTick = 0;
            m_knockbackVelocity = Vector3.zero;
        }
        if(packet.instigator)
        {
            SetTarget(packet.instigator.transform);
        }
    }

    void OnHitNoDamage(DamagePacket packet)
    {
        if (packet.instigator)
        {
            SetTarget(packet.instigator.transform);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(this.transform.position, m_aggroRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, m_attackRange);
    }
}
