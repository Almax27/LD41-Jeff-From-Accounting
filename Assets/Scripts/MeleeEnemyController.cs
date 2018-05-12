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
public class MeleeEnemyController : EnemyController {

    public bool m_aggroOnDamage = true;
    public bool m_aggroOnHitNoDamage = true;
    public float m_aggroRange = 10;
    public float m_attackRange = 3.0f;
    public float m_attackRate = 1.0f;

    public float m_knockbackSpeed = 5.0f;
    public float m_knockbackDuration = 0.1f;
    public float m_knockbackStunDuration = 0.25f;

    public AnimationCurve m_shakeCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.25f, 1f),
                                                            new Keyframe(0.5f, 0f), new Keyframe(0.75f, -1f),
                                                            new Keyframe(1f, 0f)); // sin curve for head bob
    public Transform transformToShake = null;
    public float shakeMag = 0.05f;

    public float m_bobSpeed = 2.0f;
    public float m_bobSpeedVariance = 0.5f;
    public float m_bobHeight = 0.2f;

    public GameObject m_eyes = null;
    public AudioSource m_ambientAudioSource = null;

    Vector3 originalShakePos = Vector3.zero;

    float m_baseSpeed;

    Transform m_target = null;
    NavMeshAgent m_agent = null;
    Health m_health = null;

    MeleeEnemyState m_state = MeleeEnemyState.Moving;

    float m_bobTick = 0.0f;
    float m_attackTick = 0.0f;
    float m_knockbackTick = 0.0f;
    Vector3 m_knockbackTargetPosition = Vector3.zero;
    Vector3 m_knockbackVelocity = Vector3.zero;

    public override void SetTarget(Transform target)
    {
        base.SetTarget(target);
        m_target = target;

        if (m_eyes)
        {
            m_eyes.SetActive(m_target != null);
        }
        if (m_health)
        {
            m_health.m_ignoreRaycat = m_target == null;
        }
        if (m_ambientAudioSource)
        {
            m_ambientAudioSource.enabled = m_target != null;
        }
    }

    // Use this for initialization
    void Awake () {
        m_agent = GetComponent<NavMeshAgent>();
        m_health = GetComponent<Health>();
        if (transformToShake)
        {
            originalShakePos = transformToShake.localPosition;
        }
        m_bobSpeed += Random.Range(-m_bobSpeedVariance, m_bobSpeedVariance);
        m_attackTick = m_attackRate;
        m_shakeCurve.postWrapMode = WrapMode.Loop;
        if (m_agent)
        {
            m_baseSpeed = m_agent.speed;
        }
    }

    private void OnEnable()
    {
        SetTarget(null);
        SetState(MeleeEnemyState.Moving, true);
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
                SetTarget(player.transform);
            }
        }

        //Update state
        switch (m_state)
        {
            case MeleeEnemyState.Moving:
                UpdateMoving(Time.deltaTime);
                break;
            case MeleeEnemyState.Knockback:
                UpdateKnockback();
                break;
            default:
                break;
        }
    }

    void SetState(MeleeEnemyState newState, bool force = false)
    {
        if (!force && m_state == newState) return;
        m_state = newState;
        switch (m_state)
        {
            case MeleeEnemyState.Moving:
                if (m_agent)
                {
                    m_agent.isStopped = false;
                }
                break;
            case MeleeEnemyState.Knockback:
                if (m_agent)
                {
                    m_agent.isStopped = true;
                }
                break;
            default:
                break;
        }
    }

    void UpdateMoving(float deltaTime)
    {      
        if (m_target)
        {
            Vector3 targetVector = m_target.position - transform.position;
            float targetDistance = targetVector.magnitude;
            if (targetVector.sqrMagnitude < m_attackRange * m_attackRange)
            {
                targetVector.y = 0;
                targetVector.Normalize();
                Quaternion targetRotation = Quaternion.LookRotation(targetVector, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 180.0f * deltaTime);

                m_attackTick += deltaTime;
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
            else
            {
                m_attackTick = 0;

                //move to attack range
                if (m_agent)
                {
                    m_agent.speed = m_baseSpeed;
                    m_agent.SetDestination(m_target.position);
                }
            }

            if (transformToShake)
            {
                m_bobTick += deltaTime;
                Vector3 pos = originalShakePos;
                pos += Vector3.up * Easing.Ease01(Mathf.PingPong(m_bobTick * m_bobSpeed, 1.0f), Easing.Method.QuadInOut) * m_bobHeight;
                if (m_attackTick > 0 && m_attackTick < m_attackRate)
                {
                    float shake = m_shakeCurve.Evaluate(m_attackTick / m_attackRate);
                    pos += new Vector3(0.7f, 1.2f, 0.5f) * shake * shakeMag;
                }
                transformToShake.localPosition = pos;
            }
        }
    }

    void UpdateKnockback()
    {
        m_knockbackTick += Time.deltaTime;
        if (m_knockbackTick < m_knockbackDuration)
        {
            transform.position = Vector3.SmoothDamp(transform.position, m_knockbackTargetPosition, ref m_knockbackVelocity, m_knockbackDuration, m_knockbackSpeed);
            //transform.position = Vector3.MoveTowards(transform.position, m_knockbackTargetPosition, m_knockbackSpeed * Time.deltaTime);
        }
        else if(m_knockbackTick < m_knockbackStunDuration)
        {
            //stunned
        }
        else
        {
            SetState(MeleeEnemyState.Moving);
        }
    }

    void OnDamage(DamagePacket packet)
    {
        if (m_knockbackDuration > 0)
        {
            SetState(MeleeEnemyState.Knockback);
            m_knockbackTargetPosition = transform.position - packet.hitNormal * (m_knockbackSpeed / m_knockbackDuration);
            NavMeshHit hit;
            if (NavMesh.Raycast(transform.position, m_knockbackTargetPosition, out hit, NavMesh.AllAreas))
            {
                m_knockbackTargetPosition = hit.position;
            }
            m_knockbackTick = 0;
            m_knockbackVelocity = Vector3.zero;
        }
        if(m_aggroOnDamage && packet.instigator)
        {
            SetTarget(packet.instigator.transform);
        }
    }

    void OnHitNoDamage(DamagePacket packet)
    {
        if (m_aggroOnHitNoDamage && packet.instigator)
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
