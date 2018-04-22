using UnityEngine;
using System.Collections;

public class CameraFacingBillboard : MonoBehaviour
{
    public Camera m_Camera;

    private void Start()
    {
        AlignToCamera();
    }

    void Update()
    {
        AlignToCamera();
    }

    void AlignToCamera()
    {
        if (m_Camera == null)
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
