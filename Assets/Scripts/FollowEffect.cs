using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowEffect : MonoBehaviour {

    public Transform target = null;
    public Vector3 offset = Vector3.zero;

    bool cleaningUp = false;

    private void Start()
    {
        if (!cleaningUp && target)
        {
            this.transform.position = target.position + offset;
            foreach (var trail in GetComponentsInChildren<TrailRenderer>())
            {
                trail.Clear();
            }
        }
    }

    public void Cleanup()
    {
        cleaningUp = true;
    }

    private void LateUpdate()
    {
        if (!cleaningUp && target)
        {
            this.transform.position = target.position + offset;
        }
        else
        {
            bool destroy = true;
            foreach (var ps in GetComponentsInChildren<ParticleSystem>())
            {
                if (ps.IsAlive())
                {
                    destroy = false;
                    break;
                }
            }
            foreach (var trail in GetComponentsInChildren<TrailRenderer>())
            {
                if (trail.positionCount > 1)
                {
                    destroy = false;
                    break;
                }
            }
            if (destroy)
            {
                Destroy(gameObject);
            }
        }
    }
}
