using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoHelper : MonoBehaviour {

    public bool alwaysDraw = true;
    public Color color = Color.magenta;
    public float radius = 5;

    private void OnDrawGizmos()
    {
        if (alwaysDraw)
        {
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
