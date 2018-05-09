using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class ProjectileController : MonoBehaviour {

    public float speed = 100.0f;
    public float maxDistance = 100.0f;
    public float damageRadius = 1.0f;
    public char letter = ' ';
    public LayerMask hitMask = new LayerMask();
    public LayerMask damageMask = new LayerMask();
    public GameObject instigator = null;
    public Action<bool> hitCallback = null;

    public TextMeshPro text = null;

    public List<FollowEffect> effectPrefabs = new List<FollowEffect>();
    public Spawner onHitSpawner = new Spawner();
    public Spawner onDamagedSpawner = new Spawner();

    float m_distanceTraveled = 0.0f;
    List<FollowEffect> m_activeEffects = new List<FollowEffect>();
    Vector3 m_direction = Vector3.zero;
    Vector3 m_castPosition = Vector3.zero;
    Vector3 m_castDirection = Vector3.zero;

    private void Start()
    {
        if (text)
        {
            text.text = letter.ToString();
        }
    }

    // Use this for initialization
    void OnEnable () {
		if(text)
        {
            text.text = letter.ToString();
        }
        foreach(FollowEffect prefab in effectPrefabs)
        {
            if (prefab)
            {
                GameObject gobj = Instantiate<GameObject>(prefab.gameObject);
                if (gobj)
                {
                    FollowEffect effect = gobj.GetComponent<FollowEffect>();
                    effect.target = this.transform;
                    m_activeEffects.Add(effect);
                }
            }
        }
	}

    private void OnDisable()
    {
        foreach(FollowEffect effect in m_activeEffects)
        {
            if(effect)
            {
                effect.Cleanup();
            }
        }
        m_activeEffects.Clear();
    }

    void Update ()
    {
        //move the renderable forwards
        //There will be desynchronisation with the cast due to different starting position
        //However this should be relatively minimal and will converge on the target
        float distanceToMove = speed * Time.deltaTime;
        transform.position += m_direction * distanceToMove;        
    }

    private void FixedUpdate()
    {
        float distanceToMove = speed * Time.fixedDeltaTime;
        m_distanceTraveled += distanceToMove;

        if (m_distanceTraveled > maxDistance)
        {
            OnExpired();
        }

        Vector3 castMoveDelta = m_castDirection * distanceToMove;
        Vector3 traceOrigin = m_castPosition; 
        
        //trace from one frame back to avoid missing objects coming towards us
        if(distanceToMove < m_distanceTraveled)
        {
            traceOrigin -= castMoveDelta;
        }

        //update move the cast position forwards
        m_castPosition += castMoveDelta;

        //do cast
        float castDistance = Vector3.Distance(traceOrigin, m_castPosition);
        RaycastHit hitInfo;
        bool validHit = Physics.Raycast(traceOrigin, m_castDirection, out hitInfo, castDistance, hitMask);
        RaycastHit damageHitInfo;
        if (Physics.SphereCast(traceOrigin, damageRadius, m_castDirection, out damageHitInfo, castDistance, damageMask))
        {
            hitInfo = damageHitInfo;
        }
        if (hitInfo.collider)
        {
            OnHit(hitInfo);
        }
    }

    public void OnSpawn(Vector3 muzzlePosition, Vector3 castPosition, Vector3 targetPosition)
    {
        transform.position = muzzlePosition;
        m_direction = (targetPosition - muzzlePosition).normalized;
        m_castPosition = castPosition;
        m_castDirection = (targetPosition - castPosition).normalized;
        Debug.Log(string.Format("[{0}] ProjectileSpawn", letter));
    }

    public void OnHit(RaycastHit hitInfo)
    {
        Debug.Assert(hitInfo.collider);

        transform.position = hitInfo.point;

        bool dealtDamage = false;
        Health health = hitInfo.collider.GetComponentInParent<Health>();
        if (health)
        {
            if (health.TakeDamage(new DamagePacket(instigator, -m_direction, false, letter)))
            {
                onDamagedSpawner.ProcessSpawns(transform, hitInfo.point, Quaternion.LookRotation(hitInfo.normal), Vector3.one);
                dealtDamage = true;
                DebugExtension.DebugWireSphere(hitInfo.point, Color.red, damageRadius, 10.0f);
            }
        }
        if (!dealtDamage)
        {
            onHitSpawner.ProcessSpawns(transform, hitInfo.point, Quaternion.LookRotation(hitInfo.normal), Vector3.one);
            DebugExtension.DebugWireSphere(hitInfo.point, Color.grey, damageRadius, 10.0f);
        }

        Debug.Log(string.Format("[{0}] Hit{1}: {2}", letter, dealtDamage ? "+Damage" : "", health ? health.gameObject.name : hitInfo.collider.gameObject.name));

        Destroy(gameObject);
    }

    void OnExpired()
    {
        Destroy(gameObject);
    }
}
