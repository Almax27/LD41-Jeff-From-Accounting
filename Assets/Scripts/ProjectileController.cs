using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class ProjectileController : MonoBehaviour {

    public float speed = 100.0f;
    public float maxDistance = 100.0f;
    public Vector3 direction = Vector3.zero;
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

    // Update is called once per frame
    void LateUpdate ()
    {
        direction.Normalize();

        //move projectile
        Vector3 preMovePosition = transform.position;
        Vector3 newPosition = preMovePosition + direction * speed * Time.deltaTime;
        transform.position = newPosition;

        Vector3 moveDelta = newPosition - preMovePosition;
        float distanceMovedThisFrame = moveDelta.magnitude;
        m_distanceTraveled += distanceMovedThisFrame;

        //do movement cast
        RaycastHit hitInfo;
        if(!Physics.SphereCast(preMovePosition, damageRadius, direction, out hitInfo, distanceMovedThisFrame, damageMask))
        {
            Physics.Raycast(preMovePosition, direction, out hitInfo, distanceMovedThisFrame, hitMask);
        }
        if(hitInfo.collider)
        {
            OnHit(hitInfo);
        }
        if(m_distanceTraveled > maxDistance)
        {
            OnExpired();
        }
    }

    public void OnHit(RaycastHit hitInfo)
    {
        Debug.Assert(hitInfo.collider);
        bool dealtDamage = false;
        Health health = hitInfo.collider.GetComponentInParent<Health>();
        if (health)
        {
            DamagePacket packet = new DamagePacket();
            packet.instigator = instigator;
            packet.letter = letter;
            if (health.TakeDamage(packet))
            {
                onDamagedSpawner.ProcessSpawns(transform, hitInfo.point, Quaternion.LookRotation(hitInfo.normal), Vector3.one);
                dealtDamage = true;
            }
        }
        if (!dealtDamage)
        {
            onHitSpawner.ProcessSpawns(transform, hitInfo.point, Quaternion.LookRotation(hitInfo.normal), Vector3.one);
        }
        transform.position = hitInfo.point;
        Destroy(gameObject);
        DebugExtension.DebugWireSphere(hitInfo.point, damageRadius, 1.0f);
    }

    void OnExpired()
    {
        Destroy(gameObject);
    }
}
