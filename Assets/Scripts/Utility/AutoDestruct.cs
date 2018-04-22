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
