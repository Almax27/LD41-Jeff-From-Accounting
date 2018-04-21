using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowOther : MonoBehaviour
{
    public Transform target = null;

    [Header("Translation")]
    public float moveSmoothTime = 0.5f;
    public float overshootSmoothTime = 0.5f;

    [Header("Rotation")]
    public float turnSmoothTime = 0.1f;

    Vector3 currentVelocity = Vector3.zero;
    Vector3 lastTargetPosition = Vector3.zero;
    Vector3 overshoot = Vector3.zero;
    Vector3 overshootVelocity = Vector3.zero;

    Vector3 rotationalVelocity = Vector3.zero;

    // Use this for initialization
    void Start()
    {
        Vector3 initialPosition = transform.position;
        if (target)
        {
            initialPosition = target.position;
        }
        lastTargetPosition = initialPosition;
        transform.position = initialPosition;
    }

    private void OnEnable()
    {
        Reset();
    }

    private void Reset()
    {
        currentVelocity = Vector3.zero;
        //lastTargetPosition = Vector3.zero; //do not reset last target veloicity to avoid heavy overshoot
        overshoot = Vector3.zero;
        overshootVelocity = Vector3.zero;
        rotationalVelocity = Vector3.zero;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target)
        {
            Vector3 currentPosition = transform.position;
            Vector3 targetPosition = target.position;
            Vector3 targetVelocity = (targetPosition - lastTargetPosition) / Time.deltaTime;
            lastTargetPosition = targetPosition;

            //overshoot target relative to smooth damping so we work towards velocity synchronisation
            overshoot = Vector3.SmoothDamp(overshoot, targetVelocity * moveSmoothTime, ref overshootVelocity, overshootSmoothTime);

            //smooth position
            currentPosition = Vector3.SmoothDamp(currentPosition, targetPosition + overshoot, ref currentVelocity, moveSmoothTime);

            Vector3 currentRotation = transform.rotation.eulerAngles;
            Vector3 targetRotation = target.rotation.eulerAngles;
            currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationalVelocity, turnSmoothTime);

            transform.SetPositionAndRotation(currentPosition, Quaternion.Euler(currentRotation));

            //DebugExtension.DebugPoint(targetPosition + overshoot, Color.red, 0.5f, 0, false);
            //DebugExtension.DebugPoint(targetPosition, Color.red, 0.5f, 0, false);
        }
    }
}
