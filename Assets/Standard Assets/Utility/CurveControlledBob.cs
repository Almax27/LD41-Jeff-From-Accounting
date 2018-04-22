using System;
using UnityEngine;


namespace UnityStandardAssets.Utility
{
    [Serializable]
    public class CurveControlledBob
    {
        public float HorizontalBobRange = 0.33f;
        public float VerticalBobRange = 0.33f;
        public AnimationCurve BobCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
                                                            new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
                                                            new Keyframe(2f, 0f)); // sin curve for head bob
        public float VerticalToHorizontalRatio = 2.0f;

        private Vector3 m_OriginalCameraPosition;
        private float m_tValX = 0;
        private float m_tValY = 0;


        public void Setup(Camera camera)
        {
            m_OriginalCameraPosition = camera.transform.localPosition;
        }

        public Vector3 UpdateBob(float deltaTVal)
        {
            m_tValX += deltaTVal;
            m_tValY += deltaTVal * VerticalToHorizontalRatio;
            float xPos = m_OriginalCameraPosition.x + (BobCurve.Evaluate(m_tValX) * HorizontalBobRange);
            float yPos = m_OriginalCameraPosition.y + (BobCurve.Evaluate(m_tValY) * VerticalBobRange);
            return new Vector3(xPos, yPos, 0f);
        }
    }
}
