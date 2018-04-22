using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ProjectileController : MonoBehaviour {

    public float destroyOnHitDelay = 0.1f;
    public float speed = 100.0f;
    public float maxDistance = 100.0f;
    public Vector3 direction = Vector3.zero;
    public float radius = 1.0f;
    public char letter = ' ';
    public LayerMask layerMask = new LayerMask();
    public GameObject instigator = null;
    public Action<bool> hitCallback = null;

    public TextMesh textMesh = null;

    float m_distanceTraveled = 0.0f;
    

    // Use this for initialization
    void Start () {
		if(textMesh)
        {
            textMesh.text = letter.ToString();
        }
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
        RaycastHit hitInfo;
        if(Physics.SphereCast(preMovePosition, radius, direction, out hitInfo, distanceMovedThisFrame, layerMask))
        {
            Health health = hitInfo.collider.GetComponent<Health>();
            if(health)
            {
                DealDamage(health);
            }
            transform.position = hitInfo.point;
            if (destroyOnHitDelay >= 0)
            {
                Destroy(gameObject, destroyOnHitDelay);
            }
            DebugExtension.DebugWireSphere(hitInfo.point, radius, 1.0f);
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
