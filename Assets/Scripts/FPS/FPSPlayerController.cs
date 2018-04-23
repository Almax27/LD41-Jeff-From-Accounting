using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

public enum GroundType
{
    Default
}

public enum PlayerMoveState
{
    Idle,
    GroundedMove,
    Jumping,
    Falling
}

[RequireComponent(typeof(CharacterController))]
public class FPSPlayerController : MonoBehaviour
{
    public GunController m_gunController = null;

    public float m_WalkSpeed;
    public float m_RunSpeed;
    [Range(0f, 1f)] public float m_RunStepScale = 0.7f;
    public float m_JumpSpeed;
    public float m_StickToGroundForce;
    public float m_GravityMultiplier;
    public MouseLook m_MouseLook;
    public bool m_UseFovKick;
    public FOVKick m_FovKick = new FOVKick();
    public bool m_UseHeadBob;
    public float m_headBobBlendRate = 0.1f;
    public CurveControlledBob m_HeadBob = new CurveControlledBob();
    public LerpControlledBob m_JumpBob = new LerpControlledBob();
    public float m_StepCycleDistance = 1.0f;
    public FootAudioController m_FootAudio;

    private PlayerMoveState m_moveState = PlayerMoveState.Idle;
    private float m_maxSpeed = 0.0f;
    private Vector3 m_desiredVelocity = Vector3.zero;

    private Camera m_Camera;
    private float m_YRotation;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private Vector3 m_OriginalCameraPosition;
    private float m_stepTravel;
    private float m_bobBlend = 0.0f;

    // Use this for initialization
    private void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Camera = Camera.main;
        m_OriginalCameraPosition = m_Camera.transform.localPosition;
        m_FovKick.Setup(m_Camera);
        m_HeadBob.Setup(m_Camera);
        m_stepTravel = 0f;
        m_MouseLook.Init(transform, m_Camera.transform);
    }


    // Update is called once per frame
    private void Update()
    {
        RotateView();

        // the jump state needs to read here to make sure it is not missed
        if(CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            TryJump();
        }
    }

    private void FixedUpdate()
    {
        PlayerMoveState prevMoveState = m_moveState;
        bool runStarted = Input.GetKeyDown(KeyCode.LeftShift);
        bool runEnded = Input.GetKeyUp(KeyCode.LeftShift);
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        bool wasGrounded = m_CharacterController.isGrounded;

        m_maxSpeed = isRunning ? m_RunSpeed : m_WalkSpeed;

        Vector3 inputVelocity = CalculateInputVelocity();
        
        if (wasGrounded && m_moveState != PlayerMoveState.Jumping)
        {
            if(m_moveState == PlayerMoveState.Falling)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                if (m_FootAudio) m_FootAudio.OnLand();
            }

            m_desiredVelocity = inputVelocity;

            m_desiredVelocity.y = -m_StickToGroundForce;
        }
        else
        {
            Vector3 gravitionalVelocity = Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
            inputVelocity += gravitionalVelocity;
            m_desiredVelocity += gravitionalVelocity;

            m_desiredVelocity.x = inputVelocity.x;
            m_desiredVelocity.z = inputVelocity.z;

            if(m_desiredVelocity.y > 0)
            {
                RaycastHit hitInfo;
                if(Physics.SphereCast(  transform.position, m_CharacterController.radius, Vector3.up, out hitInfo,
                                        m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    m_moveState = PlayerMoveState.Falling;
                    m_desiredVelocity.y = 0;
                }
            }
        }

        m_CollisionFlags = m_CharacterController.Move(m_desiredVelocity * Time.fixedDeltaTime);

        float speed = m_CharacterController.velocity.magnitude;

        if(m_CharacterController.isGrounded && wasGrounded)
        {
            m_moveState = speed > 0.0f ? PlayerMoveState.GroundedMove : PlayerMoveState.Idle;
        }
        else if (m_CharacterController.velocity.y < 0)
        {
            m_moveState = PlayerMoveState.Falling;
        }

        float stepDistance = speed * Time.fixedDeltaTime * (isRunning ? m_RunStepScale : 1.0f);
        ProgressStepCycle(isRunning, stepDistance, prevMoveState);
        UpdateCameraPosition(speed, stepDistance);

        m_MouseLook.UpdateCursorLock();

        // handle speed change to give an fov kick
        // only if the player is going to a run, is running and the fovkick is to be used
        if ((runStarted || runEnded) && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
        {
            StopAllCoroutines();
            StartCoroutine(!isRunning ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
        }
    }

    private bool IsMoveAllowed()
    {
        if(m_gunController && m_gunController.GetIsReloading())
        {
            return false;
        }
        return true;
    }

    private void TryJump()
    {
        if(m_CharacterController.isGrounded && IsMoveAllowed())
        {
            m_moveState = PlayerMoveState.Jumping;
            m_desiredVelocity.y = m_JumpSpeed;
            if (m_FootAudio) m_FootAudio.OnJump();
        }
    }

    private void ProgressStepCycle(bool isRunning, float stepDistance, PlayerMoveState prevMoveState)
    {
        if(stepDistance > m_StepCycleDistance)
        {
            m_stepTravel = 0;
            OnStep();
        }
        else if (stepDistance > 0.001f)
        {
            m_stepTravel += stepDistance;
            if (m_stepTravel > m_StepCycleDistance)
            {
                m_stepTravel -= m_StepCycleDistance;
                OnStep();
            }
            //do step when moving from idle
            else if (prevMoveState == PlayerMoveState.Idle)
            {
                OnStep();
            }
        }
        else
        {
            m_stepTravel = 0;
        }
    }

    void OnStep()
    {
        if (m_FootAudio && m_CharacterController.isGrounded)
        {
            m_FootAudio.OnStep();
        }
    }


    private void UpdateCameraPosition(float speed, float stepDistance)
    {
        if (!m_UseHeadBob)
        {
            return;
        }

        Vector3 BobPosition = m_HeadBob.UpdateBob(stepDistance / m_StepCycleDistance);
        Vector3 NoBobPosition = m_Camera.transform.localPosition;
        NoBobPosition.y = m_OriginalCameraPosition.y;

        bool IsHeadBobbing = m_moveState == PlayerMoveState.GroundedMove;
        m_bobBlend = Mathf.Clamp01(m_bobBlend + (Time.deltaTime / m_headBobBlendRate) * (IsHeadBobbing ? 1 : -1));

        float easedBlend = Easing.Ease01(m_bobBlend, Easing.Method.QuadInOut);
        Vector3 newCameraPosition = Vector3.Lerp(NoBobPosition, BobPosition, easedBlend);
        newCameraPosition.y -= m_JumpBob.Offset();
        m_Camera.transform.localPosition = newCameraPosition;
    }


    private Vector3 CalculateInputVelocity()
    {
        if(!IsMoveAllowed())
        {
            return Vector3.zero;
        }

        // Read input
        Vector3 input = Vector3.zero;
        input.x = CrossPlatformInputManager.GetAxis("Horizontal");
        input.y = CrossPlatformInputManager.GetAxis("Vertical");
        input.Normalize();

        //character facing relative movement
        Vector3 move = transform.forward * input.y + transform.right * input.x;

        // get a normal for the surface that is being stood on to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                           m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        move = Vector3.ProjectOnPlane(move, hitInfo.normal).normalized;

        return move * m_maxSpeed;
    }


    private void RotateView()
    {
        m_MouseLook.LookRotation(transform, m_Camera.transform);
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }
        body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }
}