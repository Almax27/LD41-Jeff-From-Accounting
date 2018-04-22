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
    void Update ()
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
        bool hitFound = false;
        RaycastHit hitInfo;
        if (Physics.SphereCast(preMovePosition, damageRadius, direction, out hitInfo, distanceMovedThisFrame, damageMask))
        {
            Health health = hitInfo.collider.GetComponent<Health>();
            if (health)
            {
                DealDamage(health);
                hitFound = true;
            }
        }
        if(!hitFound)
        {
            if(Physics.Raycast(preMovePosition, direction, out hitInfo, distanceMovedThisFrame, hitMask))
            {
                hitFound = true;
            }
        }
        if(hitFound)
        {
            transform.position = hitInfo.point;
            Destroy(gameObject);
            DebugExtension.DebugWireSphere(hitInfo.point, damageRadius, 1.0f);
        }

        if(m_distanceTraveled > maxDistance)
        {
            OnExpired();
        }
    }

    void DealDamage(Health health)
    {
        DamagePacket packet = new DamagePacket();
        packet.instigator = instigator;
        packet.letter = letter;
        health.TakeDamage(packet);
    }

    void OnExpired()
    {
        Destroy(gameObject);
    }
}
