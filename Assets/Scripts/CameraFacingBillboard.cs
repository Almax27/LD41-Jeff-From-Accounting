using UnityEngine;
using System.Collections;

public class CameraFacingBillboard : MonoBehaviour
{
    public Camera m_Camera;

    void Update()
    {
        if(m_Camera == null)
        {
            m_Camera = Camera.main;
        }
        if (m_Camera)
        {
            Vector3 forward = m_Camera.transform.forward;
            forward.y = 0;
            forward.Normalize();

            transform.LookAt(transform.position + forward, Vector3.up);
        }
    }
}
