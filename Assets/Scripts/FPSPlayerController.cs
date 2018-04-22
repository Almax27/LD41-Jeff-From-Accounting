using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using UnityStandardAssets.Characters.FirstPerson;
using Random = UnityEngine.Random;

public enum GroundType
{
    Default
}

public enum PlayerMoveState
{
    Grounded,
    Jumping,
    Falling
}

[RequireComponent(typeof(CharacterController))]
public class FPSPlayerController : MonoBehaviour
{
    public GunController m_gunController = null;

    public float m_WalkSpeed;
    public float m_RunSpeed;
    [Range(0f, 1f)] public float m_RunstepScale = 0.7f;
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
    public float m_StepInterval;
    public FootAudioController m_FootAudio;

    private PlayerMoveState m_moveState = PlayerMoveState.Grounded;
    private float m_maxSpeed = 0.0f;
    private Vector3 m_velocity = Vector3.zero;
    private Vector3 m_acceleration = Vector3.zero;


    private Camera m_Camera;
    private bool m_Jump;
    private float m_YRotation;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private Vector3 m_OriginalCameraPosition;
    private float m_StepCycle;
    private float m_NextStep;
    private bool m_Jumping;
    private float m_bobBlend = 0.0f;

    // Use this for initialization
    private void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Camera = Camera.main;
        m_OriginalCameraPosition = m_Camera.transform.localPosition;
        m_FovKick.Setup(m_Camera);
        m_HeadBob.Setup(m_Camera, m_StepInterval);
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f;
        m_Jumping = false;
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

        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            StartCoroutine(m_JumpBob.DoBobCycle());
            if (m_FootAudio) m_FootAudio.OnLand();
            m_Jumping = false;
        }

        m_PreviouslyGrounded = m_CharacterController.isGrounded;
    }


    private void FixedUpdate()
    {
        bool runStarted = Input.GetKeyDown(KeyCode.LeftShift);
        bool runEnded = Input.GetKeyUp(KeyCode.LeftShift);
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        m_maxSpeed = isRunning ? m_RunSpeed : m_WalkSpeed;

        Vector3 desiredVelocity = CalculateDesiredVelocity();
        
        if (m_CharacterController.isGrounded && m_moveState != PlayerMoveState.Jumping)
        {
            m_moveState = PlayerMoveState.Grounded;

            //m_velocity = Vector3.SmoothDamp(m_velocity, desiredVelocity, ref m_acceleration, 0.1f);
            m_velocity = desiredVelocity;

            m_velocity.y = -m_StickToGroundForce;
        }
        else
        {
            Vector3 gravitionalVelocity = Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
            desiredVelocity += gravitionalVelocity;
            m_velocity += gravitionalVelocity;

            //m_velocity.x = Mathf.SmoothDamp(m_velocity.x, desiredVelocity.x, ref m_acceleration.x, 0.1f);
            //m_velocity.y = Mathf.SmoothDamp(m_velocity.y, desiredVelocity.y, ref m_acceleration.y, 0.1f);
            m_velocity.x = desiredVelocity.x;
            m_velocity.z = desiredVelocity.z;
            m_acceleration.z = 0;

            if (m_moveState == PlayerMoveState.Jumping && m_velocity.y <= 0)
            {
                m_moveState = PlayerMoveState.Falling;
            }
        }
        m_CollisionFlags = m_CharacterController.Move(m_velocity * Time.fixedDeltaTime);

        float speed = m_velocity.magnitude;
        ProgressStepCycle(isRunning, speed);
        UpdateCameraPosition(isRunning, speed);

        m_MouseLook.UpdateCursorLock();

        // handle speed change to give an fov kick
        // only if the player is going to a run, is running and the fovkick is to be used
        if ((runStarted || runEnded) && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
        {
            StopAllCoroutines();
            StartCoroutine(!isRunning ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
        }
    }

    private void TryJump()
    {
        if(m_moveState == PlayerMoveState.Grounded)
        {
            m_moveState = PlayerMoveState.Jumping;
            m_velocity.y = m_JumpSpeed;
            if (m_FootAudio) m_FootAudio.OnJump();
        }
    }

    private void ProgressStepCycle(bool isRunning, float speed)
    {
        if (m_CharacterController.velocity.sqrMagnitude > 0)
        {
            m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (isRunning ? m_RunstepScale : 1.0f))) *
                         Time.fixedDeltaTime;
        }

        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + m_StepInterval;

        if(m_FootAudio && m_CharacterController.isGrounded)
        {
            //m_FootstepAudio.m_groundType = 
            m_FootAudio.OnStep();
        }
    }


    private void UpdateCameraPosition(bool isRunning, float speed)
    {
        if (!m_UseHeadBob)
        {
            return;
        }

        Vector3 BobPosition = m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude + (speed * (isRunning ? m_RunstepScale : 1.0f)));
        Vector3 NoBobPosition = m_Camera.transform.localPosition;
        NoBobPosition.y = m_OriginalCameraPosition.y;

        bool IsHeadBobbing = m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded;
        m_bobBlend = Mathf.Clamp01(m_bobBlend + (Time.deltaTime / m_headBobBlendRate) * (IsHeadBobbing ? 1 : -1));

        Vector3 newCameraPosition = Vector3.Lerp(NoBobPosition, BobPosition, m_bobBlend);
        newCameraPosition.y -= m_JumpBob.Offset();
        m_Camera.transform.localPosition = newCameraPosition;
    }


    private Vector3 CalculateDesiredVelocity()
    {
        if(m_gunController && m_gunController.GetIsReloading())
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