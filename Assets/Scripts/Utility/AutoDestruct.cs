using UnityEngine;
using System.Collections;

public enum AutoDestructMode
{
    Destroy,
    Deactivate
}

public class AutoDestruct : MonoBehaviour {

    public AutoDestructMode mode = AutoDestructMode.Destroy;
    public float delay;
    public bool waitForParticles = true;
    private float tick;

    private void OnEnable()
    {
        foreach (var ps in GetComponentsInChildren<ParticleSystem>())
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play(true);
        }
    }

    // Update is called once per frame
    void Update () 
    {
        if (tick > delay)
        {
            bool destruct = true;
            if (waitForParticles)
            {
                foreach (var ps in GetComponentsInChildren<ParticleSystem>())
                {
                    if (ps.IsAlive())
                    {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                        destruct = false;
                        break;
                    }
                }
            }
            if (destruct)
            {
                switch (mode)
                {
                    case AutoDestructMode.Destroy:
                        Destroy(gameObject);
                        break;
                    case AutoDestructMode.Deactivate:
                        gameObject.SetActive(false);
                        break;
                }
            }
        }
        else
        {
            tick += Time.deltaTime;
        }
	}
}
