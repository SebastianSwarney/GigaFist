using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class PlayerControllerEvent : UnityEvent { }

public class PlayerController : MonoBehaviour
{
    public enum MovementControllState { MovementEnabled, MovementDisabled }
    public enum GravityState { GravityEnabled, GravityDisabled }
    public enum DamageState { Vulnerable, Invulnerable }
    public enum InputState { InputEnabled, InputDisabled }
    public PlayerState m_states;

    #region Movement Events
    public PlayerMovementEvents m_movementEvents;
    [System.Serializable]
    public struct PlayerMovementEvents
    {
        [Header("Basic Events")]
        public PlayerControllerEvent m_onLandedEvent;
        public PlayerControllerEvent m_onJumpEvent;
        public PlayerControllerEvent m_onRespawnEvent;

        [Header("Wall Run Events")]
        public PlayerControllerEvent m_onWallRunBeginEvent;
        public PlayerControllerEvent m_onWallRunEndEvent;
        public PlayerControllerEvent m_onWallRunJumpEvent;

        [Header("Wall Climb Events")]
        public PlayerControllerEvent m_onWallClimbBeginEvent;
        public PlayerControllerEvent m_onWallClimbEndEvent;
        public PlayerControllerEvent m_onWallClimbJumpEvent;

        [Header("Wall Jump Events")]
        public PlayerControllerEvent m_onWallJumpEvent;

        [Header("Leap Events")]
        public PlayerControllerEvent m_onLeapEvent;

    }
    #endregion

    #region Camera Properties
    [Header("Camera Properties")]

    public float m_mouseSensitivity;
    public float m_maxCameraAng;
    public bool m_inverted;
    public Camera m_camera;
    public Transform m_cameraTilt;
    public Transform m_cameraMain;
    [Space]
    #endregion

    #region Base Movement Properties
    [Header("Base Movement Properties")]

    public float m_baseMovementSpeed;
    public float m_accelerationTime;

    private float m_currentMovementSpeed;
    [HideInInspector]
    public Vector3 m_velocity;
    private Vector3 m_velocitySmoothing;
    private CharacterController m_characterController;

    [Space]
    #endregion

    #region Jumping Properties
    [Header("Jumping Properties")]

    public float m_maxJumpHeight = 4;
    public float m_minJumpHeight = 1;
    public float m_timeToJumpApex = .4f;

    public float m_graceTime;
    private float m_graceTimer;

    public float m_jumpBufferTime;
    private float m_jumpBufferTimer;

    public float m_gravity;
    private float m_maxJumpVelocity;
    private float m_minJumpVelocity;
    private bool m_isLanded;
    private bool m_offLedge;

    [Space]
    #endregion

    #region Leaping Properties
    [Header("Leaping Properties")]

    public AnimationCurve m_leapCurve;
    public float m_leapTime;
    public float m_leapSpeedBoostBase;
    public float m_leapSpeedBoostIncrease;
    public int m_leapSpeedBoostCountMax;
    public float m_leapBufferTime;

    private float m_currentLeapTime;
    private float m_leapingTimer;
    private float m_leapBufferTimer;
    private int m_leapCount;
    private float m_currentLeapSpeed;
    private bool m_isLeaping;

    [Space]
    #endregion

    #region Wall Run Properties
    [Header("Wall Run Properties")]

    public LayerMask m_wallMask;

    public AnimationCurve m_wallSpeedCurve;
    public float m_wallSpeedUpTime;
    public float m_maxWallRunSpeed;

    public float m_tiltSpeed;
    public float m_wallRunCameraMaxTilt;

    public int m_wallRidingRayCount;
    public float m_wallRaySpacing;
    public float m_wallRunRayLength;
    public float m_wallRunBufferTime;
    public Vector3 m_wallRunJumpVelocity;

    public float m_wallJumpBufferTime;
    public Vector3 m_wallJumpVelocity;

    private float m_currentWallRunningSpeed;

    private float m_wallRunBufferTimer;
    private float m_wallJumpBufferTimer;

    private float m_tiltTarget;
    private float m_tiltSmoothingVelocity;

    private bool m_isWallRunning;
    private bool m_connectedWithWall;
    public bool m_holdingWallRideStick;

    private Vector3 m_wallHitPos;
    private float m_wallHitDst;
    private Vector3 m_wallDir;
    private Vector3 m_wallVector;
    private Vector3 m_wallFacingVector;
    private Vector3 m_modelWallRunPos;

    [Space]
    #endregion

    #region Wall Climb Properties
    [Header("Wall Climb Properties")]

    public AnimationCurve m_wallClimbSpeedCurve;
    public float m_maxWallClimbSpeed;
    public float m_wallClimbSpeedUpTime;
    public float m_wallClimbFactor;
    public Vector3 m_wallClimbJumpVelocity;

    private float m_currentWallClimbSpeed;
    private bool m_isWallClimbing;
    [HideInInspector]
    public Vector3 m_localWallFacingVector;
    [Space]
    #endregion

    #region Slide Properties
    [Header("Slide Properties")]

    public float m_slideTime;
    public AnimationCurve m_slideCurve;
    public float m_slideSpeedUpTime;
    public float m_maxSlideSpeed;

    private float m_currentSlideSpeed;
    private bool m_isSliding;

    [Space]
    #endregion

    #region Punch Properties
    [Header("Punch Properties")]

    public LayerMask m_playerFistMask;
    public float m_minPunchTime;
    public float m_maxPunchTime;
    public float m_minPunchDistance;
    public float m_maxPunchDistance;
    public AnimationCurve m_punchCurve;
    public float m_punchChargeTime;
    public float m_punchReflectThreshold;

    public float m_punchSpeed;
    public float m_punchEndSpeed;

    public Transform m_chargeVisual;

    private bool m_isChargingPunch;
    private bool m_isPunching;
    private Vector3 m_punchHitNormal;
    private Vector3 m_punchHitPos;
    [Space]
    #endregion

    #region Uppercut Properties
    [Header("Uppercut Properties")]

    public float m_uppercutTime;
    public float m_uppercutDistance;
    public AnimationCurve m_uppercutCurve;

    private bool m_isUppercutting;
    [Space]
    #endregion

    #region Short Dash Properties
    [Header("Short Dash Properties")]

    public float m_shortDashTime;
    public float m_shortDashDistance;
    public AnimationCurve m_shortDashCurve;

    private bool m_isShortDashing;
    [Space]
    #endregion

    #region Speed Boost Properties
    [Header("Speed Boost Properties")]
    private float m_speedBoostChargeTimer;
    public float m_speedBoostChargeTime;

    private float m_speedBoostCharge;
    public float m_speedBoostChargeMax;

    public float m_speedBoostTime;
    public float m_speedBoostSpeed;
    public AnimationCurve m_speedBoostCurve;

    private bool m_isSpeedBoosting;
    private Coroutine m_speedBoostCoroutine;
    [Space]
    #endregion

    #region Slam Properties
    [Header("Slam Properties")]
    public float m_slamTime;
    public AnimationCurve m_slamCurve;

    private bool m_isSlaming;
    [Space]
    #endregion

    private Vector2 m_movementInput;
    private Vector2 m_lookInput;

    private Rigidbody m_rigidbody;
    private bool m_isStunned;

    private Coroutine m_wallJumpBufferCoroutine;
    private Coroutine m_jumpBufferCoroutine;
    private Coroutine m_graceBufferCoroutine;
    private Coroutine m_leapBufferCoroutine;
    private Coroutine m_wallRunBufferCoroutine;

    private float m_currentSpeedBoost;

    private void Start()
    {
        m_characterController = GetComponent<CharacterController>();
        m_rigidbody = GetComponent<Rigidbody>();

        CalculateJump();
        LockCursor();

        m_currentMovementSpeed = m_baseMovementSpeed;
        m_jumpBufferTimer = m_jumpBufferTime;
        m_wallJumpBufferTimer = m_wallJumpBufferTime;
        m_wallRunBufferTimer = m_wallRunBufferTime;

        m_speedBoostChargeTimer = m_speedBoostChargeTime;
        m_speedBoostCharge = m_speedBoostChargeMax;
    }

    private void OnValidate()
    {
        //CalculateJump();
    }

    private void FixedUpdate()
    {
        PerformController();
    }

    public void PerformController()
    {
        //CheckWallRun();

        CalculateCurrentSpeed();
        CalculateVelocity();

        m_characterController.Move(m_velocity * Time.deltaTime);

        CalculateGroundPhysics();

        CameraRotation();
        TiltLerp();
    }

    #region Input Code
    public void SetMovementInput(Vector2 p_input)
    {
        m_movementInput = p_input;
    }

    public void SetLookInput(Vector2 p_input)
    {
        m_lookInput = p_input;
    }

    public void WallRideInputDown()
    {
        m_holdingWallRideStick = true;
    }

    public void WallRideInputUp()
    {
        m_holdingWallRideStick = false;
        OnWallRideRelease();
    }
    #endregion

    #region Camera Code
    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ResetCamera()
    {
        m_cameraMain.rotation = Quaternion.identity;
        m_cameraTilt.rotation = Quaternion.identity;
    }

    private void CameraRotation()
    {
        //Get the inputs for the camera
        Vector2 cameraInput = new Vector2(m_lookInput.y * ((m_inverted) ? -1 : 1), m_lookInput.x);

        if (!m_isPunching)
        {
            //Rotate the player on the y axis (left and right)
            transform.Rotate(Vector3.up, cameraInput.y * (m_mouseSensitivity));
        }

        float cameraXAng = m_cameraMain.transform.eulerAngles.x;

        if (!m_isPunching)
        {
            //Stops the camera from rotating, if it hits the resrictions
            if (cameraInput.x < 0 && cameraXAng > 360 - m_maxCameraAng || cameraInput.x < 0 && cameraXAng < m_maxCameraAng + 10)
            {
                m_cameraMain.transform.Rotate(Vector3.right, cameraInput.x * (m_mouseSensitivity));

            }
            else if (cameraInput.x > 0 && cameraXAng > 360 - m_maxCameraAng - 10 || cameraInput.x > 0 && cameraXAng < m_maxCameraAng)
            {
                m_cameraMain.transform.Rotate(Vector3.right, cameraInput.x * (m_mouseSensitivity));

            }

            if (m_cameraMain.transform.eulerAngles.x < 360 - m_maxCameraAng && m_cameraMain.transform.eulerAngles.x > 180)
            {
                m_cameraMain.transform.localEulerAngles = new Vector3(360 - m_maxCameraAng, 0f, 0f);
            }
            else if (m_camera.transform.eulerAngles.x > m_maxCameraAng && m_cameraMain.transform.eulerAngles.x < 180)
            {
                m_cameraMain.transform.localEulerAngles = new Vector3(m_maxCameraAng, 0f, 0f);
            }
        }

    }
    #endregion

    #region Input Buffering Code

    private bool CheckBuffer(ref float p_bufferTimer, ref float p_bufferTime, Coroutine p_bufferTimerRoutine)
    {
        if (p_bufferTimer < p_bufferTime)
        {
            if (p_bufferTimerRoutine != null)
            {
                StopCoroutine(p_bufferTimerRoutine);
            }

            p_bufferTimer = p_bufferTime;

            return true;
        }
        else if (p_bufferTimer >= p_bufferTime)
        {
            return false;
        }

        return false;
    }

    private bool CheckOverBuffer(ref float p_bufferTimer, ref float p_bufferTime, Coroutine p_bufferTimerRoutine)
    {
        if (p_bufferTimer >= p_bufferTime)
        {
            p_bufferTimer = p_bufferTime;

            return true;
        }

        return false;
    }

    //Might want to change this so it does not feed the garbage collector monster
    private IEnumerator RunBufferTimer(System.Action<float> m_bufferTimerRef, float p_bufferTime)
    {
        float t = 0;

        while (t < p_bufferTime)
        {
            t += Time.deltaTime;
            m_bufferTimerRef(t);
            yield return null;
        }

        m_bufferTimerRef(p_bufferTime);
    }

    #endregion

    #region Player State Code
    [System.Serializable]
    public struct PlayerState
    {
        public MovementControllState m_movementControllState;
        public GravityState m_gravityControllState;
        public DamageState m_damageState;
        public InputState m_inputState;
    }

    private bool IsGrounded()
    {
        if (m_characterController.collisionFlags == CollisionFlags.Below)
        {
            return true;
        }

        return false;
    }

    private bool OnSlope()
    {
        RaycastHit hit;

        Vector3 bottom = m_characterController.transform.position - new Vector3(0, m_characterController.height / 2, 0);

        if (Physics.Raycast(bottom, Vector3.down, out hit, 0.2f))
        {
            if (hit.normal != Vector3.up)
            {
                return true;
            }
        }

        return false;
    }

    private void OnLanded()
    {
        m_isLanded = true;

        m_movementEvents.m_onLandedEvent.Invoke();

        if (CheckBuffer(ref m_jumpBufferTimer, ref m_jumpBufferTime, m_jumpBufferCoroutine))
        {
            RunLeap();
        }

        m_leapBufferCoroutine = StartCoroutine(RunBufferTimer((x) => m_leapBufferTimer = (x), m_leapBufferTime));
    }

    private void OnOffLedge()
    {
        m_offLedge = true;

        m_graceBufferCoroutine = StartCoroutine(RunBufferTimer((x) => m_graceTimer = (x), m_graceTime));

    }

    public void Respawn()
    {
        m_movementEvents.m_onRespawnEvent.Invoke();

        ResetCamera();
        m_currentLeapTime = 0;
    }
    #endregion

    #region Physics Calculation Code

    private void CalculateCurrentSpeed()
    {
        float speed = m_baseMovementSpeed;

        speed += m_currentLeapSpeed;
        speed += m_currentWallRunningSpeed;
        speed += m_currentSlideSpeed;
        speed += m_currentWallClimbSpeed;
        speed += m_currentSpeedBoost;

        m_currentMovementSpeed = speed;
    }

    public void SpeedBoost(float p_boostAmount)
    {
        m_currentSpeedBoost = p_boostAmount;
    }

    private void CalculateGroundPhysics()
    {
        if (IsGrounded() && !OnSlope())
        {
            m_velocity.y = 0;
        }

        if (OnSlope())
        {
            RaycastHit hit;

            Vector3 bottom = m_characterController.transform.position - new Vector3(0, m_characterController.height / 2, 0);

            if (Physics.Raycast(bottom, Vector3.down, out hit))
            {
                m_characterController.Move(new Vector3(0, -(hit.distance), 0));
            }
        }

        if (!IsGrounded() && !m_offLedge)
        {
            OnOffLedge();
        }
        if (IsGrounded())
        {
            m_offLedge = false;
        }

        if (IsGrounded() && !m_isLanded)
        {
            OnLanded();
        }
        if (!IsGrounded())
        {
            m_isLanded = false;
        }
    }

    private void CalculateVelocity()
    {
        if (m_states.m_gravityControllState == GravityState.GravityEnabled)
        {
            m_velocity.y += m_gravity * Time.deltaTime;
        }

        if (m_states.m_movementControllState == MovementControllState.MovementEnabled)
        {
            Vector2 input = new Vector2(m_movementInput.x, m_movementInput.y);

            Vector3 forwardMovement = transform.forward * input.y;
            Vector3 rightMovement = transform.right * input.x;

            Vector3 targetHorizontalMovement = Vector3.ClampMagnitude(forwardMovement + rightMovement, 1.0f) * m_currentMovementSpeed;
            Vector3 horizontalMovement = Vector3.SmoothDamp(m_velocity, targetHorizontalMovement, ref m_velocitySmoothing, m_accelerationTime);

            m_velocity = new Vector3(horizontalMovement.x, m_velocity.y, horizontalMovement.z);
        }
        else
        {
            Vector2 input = new Vector2(0, 0);

            Vector3 forwardMovement = transform.forward * input.y;
            Vector3 rightMovement = transform.right * input.x;

            Vector3 targetHorizontalMovement = Vector3.ClampMagnitude(forwardMovement + rightMovement, 1.0f) * m_currentMovementSpeed;
            Vector3 horizontalMovement = Vector3.SmoothDamp(m_velocity, targetHorizontalMovement, ref m_velocitySmoothing, m_accelerationTime);

            m_velocity = new Vector3(horizontalMovement.x, m_velocity.y, horizontalMovement.z);
        }

    }

    public void PhysicsSeekTo(Vector3 p_targetPosition)
    {
        Vector3 deltaPosition = p_targetPosition - transform.position;
        m_velocity = deltaPosition / Time.deltaTime;
    }
    #endregion

    #region Code Not In Use

    #region Jump Code
    public void OnJumpInputDown()
    {
        m_jumpBufferCoroutine = StartCoroutine(RunBufferTimer((x) => m_jumpBufferTimer = (x), m_jumpBufferTime));

        if (CheckBuffer(ref m_leapBufferTimer, ref m_leapBufferTime, m_leapBufferCoroutine) && IsGrounded())
        {
            RunLeap();
            return;
        }

        if (CheckBuffer(ref m_wallJumpBufferTimer, ref m_wallJumpBufferTime, m_wallJumpBufferCoroutine) && !m_isWallRunning)
        {
            WallJump();
            return;
        }

        if (CheckBuffer(ref m_graceTimer, ref m_graceTime, m_graceBufferCoroutine) && !IsGrounded() && m_velocity.y <= 0f)
        {
            GroundJump();
            return;
        }

        if (m_isWallClimbing)
        {
            WallRunningJump();
            return;
        }

        if (m_isWallRunning)
        {
            WallRunningJump();
            return;
        }

        if (IsGrounded())
        {
            GroundJump();
            return;
        }

    }

    public void OnJumpInputUp()
    {
        if (m_velocity.y > m_minJumpVelocity)
        {
            JumpMinVelocity();
        }
    }

    private void CalculateJump()
    {
        m_gravity = -(2 * m_maxJumpHeight) / Mathf.Pow(m_timeToJumpApex, 2);
        m_maxJumpVelocity = Mathf.Abs(m_gravity) * m_timeToJumpApex;
        m_minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(m_gravity) * m_minJumpHeight);
    }

    private void WallJump()
    {
        m_leapingTimer = 0;

        m_movementEvents.m_onWallJumpEvent.Invoke();

        m_velocity.x = m_wallDir.x * m_wallJumpVelocity.x;
        m_velocity.y = m_wallJumpVelocity.y;
        m_velocity.z = m_wallDir.z * m_wallJumpVelocity.z;
    }

    private void WallRunningJump()
    {
        m_isWallRunning = false;

        m_movementEvents.m_onWallRunJumpEvent.Invoke();

        m_wallRunBufferCoroutine = StartCoroutine(RunBufferTimer((x) => m_wallRunBufferTimer = (x), m_wallRunBufferTime));
        m_leapingTimer = 0;
        m_velocity.x = m_wallDir.x * m_wallRunJumpVelocity.x;
        m_velocity.y = m_wallRunJumpVelocity.y;
        m_velocity.z = m_wallDir.z * m_wallRunJumpVelocity.z;
    }

    private void WallClimbingJump()
    {
        m_isWallClimbing = false;

        m_movementEvents.m_onWallClimbJumpEvent.Invoke();

        m_wallRunBufferCoroutine = StartCoroutine(RunBufferTimer((x) => m_wallRunBufferTimer = (x), m_wallRunBufferTime));
        m_leapingTimer = 0;
        m_velocity.x = m_wallDir.x * m_wallClimbJumpVelocity.x;
        m_velocity.y = m_wallClimbJumpVelocity.y;
        m_velocity.z = m_wallDir.z * m_wallClimbJumpVelocity.z;
    }

    private void GroundJump()
    {
        m_movementEvents.m_onJumpEvent.Invoke();
        JumpMaxVelocity();
    }

    private void JumpMaxVelocity()
    {
        m_velocity.y = m_maxJumpVelocity;
    }

    private void JumpMinVelocity()
    {
        m_velocity.y = m_minJumpVelocity;
    }

    private void JumpMaxMultiplied(float p_force)
    {
        m_velocity.y = m_maxJumpVelocity * p_force;
    }

    #endregion

    #region Wall Run Code

    private void CheckWallRun()
    {
        float m_angleBetweenRays = m_wallRaySpacing / m_wallRidingRayCount;
        bool anyRayHit = false;

        for (int i = 0; i < m_wallRidingRayCount; i++)
        {
            Quaternion raySpaceQ = Quaternion.Euler(0, (i * m_angleBetweenRays) - (m_angleBetweenRays * (m_wallRidingRayCount / 2)), 0);
            RaycastHit hit;

            if (Physics.Raycast(m_characterController.transform.position, raySpaceQ * transform.forward, out hit, m_wallRunRayLength, m_wallMask))
            {
                if (Vector3.Dot(hit.normal, Vector3.up) == 0)
                {
                    anyRayHit = true;

                    m_wallVector = Vector3.Cross(hit.normal, Vector3.up);
                    m_wallFacingVector = Vector3.Cross(hit.normal, m_camera.transform.forward);
                    m_wallDir = hit.normal;
                    m_wallHitPos = hit.point;
                    m_wallHitDst = hit.distance;

                    m_localWallFacingVector = m_camera.transform.InverseTransformDirection(m_wallFacingVector);

                    if (!m_connectedWithWall)
                    {
                        OnWallConnect();
                    }

                    CheckToStartWallRun();
                }

                Debug.DrawLine(m_characterController.transform.position, hit.point);
            }
        }

        if (!anyRayHit)
        {
            m_isWallRunning = false;
            m_isWallClimbing = false;
            m_connectedWithWall = false;
        }

    }

    private void OnWallConnect()
    {
        m_connectedWithWall = true;
        m_wallJumpBufferCoroutine = StartCoroutine(RunBufferTimer((x) => m_wallJumpBufferTimer = (x), m_wallJumpBufferTime));
    }

    private void TiltLerp()
    {
        m_cameraTilt.localRotation = Quaternion.Slerp(m_cameraTilt.localRotation, Quaternion.Euler(0, 0, m_tiltTarget), m_tiltSpeed);
    }

    private void OnWallRideRelease()
    {
        m_isWallRunning = false;
        m_isWallClimbing = false;
        m_wallRunBufferCoroutine = StartCoroutine(RunBufferTimer((x) => m_wallRunBufferTimer = (x), m_wallRunBufferTime));
    }

    private void CheckToStartWallRun()
    {
        if (m_holdingWallRideStick)
        {
            if (m_isWallClimbing)
            {
                return;
            }

            if (m_isWallRunning)
            {
                return;
            }

            if (m_localWallFacingVector.x >= m_wallClimbFactor)
            {
                if (!m_isWallClimbing)
                {
                    if (CheckOverBuffer(ref m_wallRunBufferTimer, ref m_wallRunBufferTime, m_wallRunBufferCoroutine))
                    {
                        StartCoroutine(WallClimbing());
                        return;
                    }
                }
            }

            if (!m_isWallRunning)
            {
                if (CheckOverBuffer(ref m_wallRunBufferTimer, ref m_wallRunBufferTime, m_wallRunBufferCoroutine))
                {
                    StartCoroutine(WallRunning());
                    return;

                }
            }
        }

    }

    private IEnumerator WallClimbing()
    {
        m_movementEvents.m_onWallClimbBeginEvent.Invoke();

        m_isWallClimbing = true;

        m_states.m_gravityControllState = GravityState.GravityDisabled;
        m_states.m_movementControllState = MovementControllState.MovementDisabled;

        m_currentWallClimbSpeed = 0;

        float t = 0;

        while (m_isWallClimbing)
        {
            t += Time.deltaTime;

            m_leapingTimer = 0;

            m_velocity = Vector3.zero;

            m_velocity.y = m_localWallFacingVector.x * m_currentMovementSpeed;

            float progress = m_wallClimbSpeedCurve.Evaluate(t / m_wallClimbSpeedUpTime);
            m_currentWallClimbSpeed = Mathf.Lerp(0f, m_maxWallClimbSpeed, progress);

            yield return null;
        }

        m_states.m_movementControllState = MovementControllState.MovementEnabled;
        m_states.m_gravityControllState = GravityState.GravityEnabled;

        m_currentWallClimbSpeed = 0;

        m_movementEvents.m_onWallClimbEndEvent.Invoke();
    }

    private IEnumerator WallRunning()
    {
        m_movementEvents.m_onWallRunBeginEvent.Invoke();

        m_isWallRunning = true;
        m_states.m_gravityControllState = GravityState.GravityDisabled;
        m_states.m_movementControllState = MovementControllState.MovementDisabled;

        m_currentWallRunningSpeed = 0;

        float t = 0;



        while (m_isWallRunning)
        {
            t += Time.deltaTime;

            m_leapingTimer = 0;

            float result = Mathf.Lerp(-m_wallRunCameraMaxTilt, m_wallRunCameraMaxTilt, m_wallFacingVector.y);
            m_tiltTarget = result;

            //m_modelRoot.localPosition = new Vector3(m_wallHitDst * Mathf.Sign(result), m_modelRoot.localPosition.y, m_modelRoot.localPosition.z);

            m_velocity = (m_wallVector * -m_wallFacingVector.y) * m_currentMovementSpeed;

            m_velocity += (transform.right * m_wallFacingVector.y) * m_currentMovementSpeed;

            m_velocity.y = 0;

            float progress = m_wallSpeedCurve.Evaluate(t / m_wallSpeedUpTime);
            m_currentWallRunningSpeed = Mathf.Lerp(0f, m_maxWallRunSpeed, progress);

            yield return null;
        }

        m_states.m_movementControllState = MovementControllState.MovementEnabled;
        m_states.m_gravityControllState = GravityState.GravityEnabled;

        //m_modelRoot.localPosition = Vector3.zero - (Vector3.up * (m_characterController.height / 2));

        m_currentWallRunningSpeed = 0;

        m_tiltTarget = 0f;

        m_movementEvents.m_onWallRunEndEvent.Invoke();
    }

    #endregion

    #region Leap Code

    private void RunLeap()
    {
        m_leapCount++;

        m_movementEvents.m_onLeapEvent.Invoke();

        JumpMaxVelocity();

        if (m_isLeaping)
        {
            m_leapingTimer = 0;
        }
        else
        {
            StartCoroutine(JumpLeap());
        }
    }

    private IEnumerator JumpLeap()
    {
        m_isLeaping = true;
        m_leapingTimer = 0;
        m_currentLeapTime = m_leapTime;

        float targetLeapSpeed;

        while (m_leapingTimer < m_currentLeapTime)
        {
            m_leapingTimer += Time.deltaTime;

            if (m_leapCount <= m_leapSpeedBoostCountMax)
            {
                targetLeapSpeed = m_leapSpeedBoostBase + (m_leapSpeedBoostIncrease * m_leapCount);
            }
            else
            {
                targetLeapSpeed = m_leapSpeedBoostBase + (m_leapSpeedBoostIncrease * m_leapSpeedBoostCountMax);
            }

            float progress = m_leapCurve.Evaluate(m_leapingTimer / m_currentLeapTime);
            m_currentLeapSpeed = Mathf.Lerp(0f, targetLeapSpeed, progress);

            yield return null;
        }

        m_leapCount = 0;
        m_currentLeapSpeed = 0;
        m_isLeaping = false;
    }

    #endregion

    #region Slide Code

    private void StartSlide()
    {
        if (!m_isSliding)
        {
            if (IsGrounded())
            {
                StartCoroutine(Slide());
            }
        }
    }

    private IEnumerator Slide()
    {
        m_isSliding = true;

        float t = 0;

        while (t < m_slideTime)
        {
            t += Time.deltaTime;

            float progress = m_slideCurve.Evaluate(t / m_slideSpeedUpTime);
            m_currentSlideSpeed = Mathf.Lerp(0f, m_maxSlideSpeed, progress);

            m_leapingTimer = 0;

            yield return null;
        }

        m_currentSlideSpeed = 0;

        m_isSliding = false;
    }

    #endregion

    #region JumpPad

    public void AddToJumpMaxVelocity(float p_amount)
    {
        JumpMaxMultiplied(p_amount);
    }

    #endregion

    #endregion

    public void OnPunchInputDown()
    {
        if (!m_isChargingPunch)
        {
            StartCoroutine(ChargePunch());
        }
    }

    public void OnPunchInputUp()
    {
        m_isChargingPunch = false;
    }

    private IEnumerator ChargePunch()
    {
        m_isChargingPunch = true;

        float t = 0;

        while (m_isChargingPunch)
        {
            t += Time.deltaTime;

            //m_chargeVisual.localScale = Vector3.Lerp(new Vector3(1, 0, 0.1f), new Vector3(1, 1, 0.1f), t / m_punchChargeTime);

            yield return null;
        }

        StartCoroutine(RunPunch(m_camera.transform.forward, t / m_punchChargeTime));

        m_isChargingPunch = false;
    }

    private IEnumerator RunPunch(Vector3 p_punchDirection, float p_punchChargePercent)
    {
        m_isPunching = true;
        m_states.m_movementControllState = MovementControllState.MovementDisabled;
        m_states.m_gravityControllState = GravityState.GravityDisabled;

        float currentPunchTime = Mathf.Lerp(m_minPunchTime, m_maxPunchTime, p_punchChargePercent);
        float currentPunchDistance = Mathf.Lerp(m_minPunchDistance, m_maxPunchDistance, p_punchChargePercent);

        float t = 0;

        Vector3 startPos = transform.position;
        Vector3 punchPos = startPos + (p_punchDirection * currentPunchDistance);

        while (t < currentPunchTime)
        {
            t += Time.fixedDeltaTime;

            float progress = m_punchCurve.Evaluate(t / currentPunchTime);

            #region Reflect
            if (m_characterController.collisionFlags == CollisionFlags.Below || m_characterController.collisionFlags == CollisionFlags.Sides)
            {
                if ((Vector3.Dot(m_punchHitNormal, p_punchDirection) * -1) < m_punchReflectThreshold)
                {
                    /*
					p_punchDirection = Vector3.Reflect(p_punchDirection, m_punchHitNormal);

					punchPos = transform.position + (p_punchDirection * (m_maxPunchDistance * (1 - progress)));
					startPos = transform.position;
					*/
                }
                else
                {
                    t = currentPunchTime;
                }
            }
            #endregion

            //float targetSpeed = Mathf.Lerp(m_punchSpeed, m_punchEndSpeed, progress);

            m_velocity = p_punchDirection * m_punchSpeed;

            //Vector3 targetPos = Vector3.Lerp(startPos, punchPos, progress);
            //PhysicsSeekTo(targetPos);

            /*
			PlayerController[] hitPlayers = CheckHitbox((m_camera.transform.forward * 5) + transform.position, 5f, m_playerFistMask);

			if (hitPlayers.Length > 0)
			{
				hitPlayers[0].TriggerKnockBack(transform.forward, 250f); //Make real number

				t = m_maxPunchTime;
			}
			*/
            yield return new WaitForFixedUpdate();
        }

        m_states.m_movementControllState = MovementControllState.MovementEnabled;
        m_states.m_gravityControllState = GravityState.GravityEnabled;
        m_isPunching = false;
    }

    public void OnShortDashInputDown(Vector3 p_dashDirection)
    {
        if (!m_isShortDashing)
        {
            StartCoroutine(RunShortDash(p_dashDirection));
        }
    }

    private IEnumerator RunShortDash(Vector3 p_dashDirection)
    {
        m_isShortDashing = true;

        m_states.m_movementControllState = MovementControllState.MovementDisabled;
        m_states.m_gravityControllState = GravityState.GravityDisabled;

        float t = 0;

        Vector3 startPos = transform.position;

        Vector3 dashPos = startPos + (p_dashDirection * m_shortDashDistance);

        while (t < m_shortDashTime)
        {
            t += Time.fixedDeltaTime;

            float progress = m_shortDashCurve.Evaluate(t / m_shortDashTime);
            Vector3 targetPos = Vector3.Lerp(startPos, dashPos, progress);
            PhysicsSeekTo(targetPos);

            yield return new WaitForFixedUpdate();
        }

        m_states.m_movementControllState = MovementControllState.MovementEnabled;
        m_states.m_gravityControllState = GravityState.GravityEnabled;

        m_isShortDashing = false;
    }

    public void OnSpeedBoostInputDown()
    {
        if (!m_isSpeedBoosting && m_speedBoostCharge > 0)
        {
            if (m_speedBoostCoroutine != null)
            {
                StopCoroutine(m_speedBoostCoroutine);
            }

            StartCoroutine(RunSpeedBoost());
        }
    }

    public void OnSpeedBoostInputUp()
    {
        if (m_isSpeedBoosting)
        {
            m_isSpeedBoosting = false;
        }
    }

    private IEnumerator ChargeSpeedBoost()
    {
        while (m_speedBoostCharge < m_speedBoostChargeMax)
        {
            m_speedBoostChargeTimer += Time.deltaTime;

            float progress = m_speedBoostChargeTimer / m_speedBoostChargeTime;

            m_speedBoostCharge = Mathf.Lerp(0, m_speedBoostChargeMax, progress);

            yield return new WaitForFixedUpdate();
        }

        m_speedBoostCharge = m_speedBoostChargeMax;
    }

    private IEnumerator RunSpeedBoost()
    {
        m_isSpeedBoosting = true;
        m_states.m_gravityControllState = GravityState.GravityDisabled;

        m_velocity.y = 0f;

        float t = 0;

        while (m_isSpeedBoosting)
        {
            m_speedBoostChargeTimer -= Time.fixedDeltaTime;
            float chargeProgress = m_speedBoostChargeTimer / m_speedBoostChargeTime;
            m_speedBoostCharge = Mathf.Lerp(0f, m_speedBoostChargeMax, chargeProgress);

            t += Time.fixedDeltaTime;
            float progress = m_speedBoostCurve.Evaluate(t / m_speedBoostTime);
            float currentSpeed = Mathf.Lerp(m_speedBoostSpeed, m_speedBoostSpeed, progress);

            Vector2 input = new Vector2(m_movementInput.x, m_movementInput.y);
            Vector3 forwardMovement = transform.forward * input.y;
            Vector3 rightMovement = transform.right * input.x;
            Vector3 targetHorizontalMovement = forwardMovement + rightMovement;
            Vector3 horizontalMovement = new Vector3(targetHorizontalMovement.x, 0f, targetHorizontalMovement.z) * currentSpeed;
            m_velocity = new Vector3(horizontalMovement.x, m_velocity.y, horizontalMovement.z);

            if (m_speedBoostCharge <= 0)
            {
                m_isSpeedBoosting = false;
                m_speedBoostCharge = 0;
            }

            yield return new WaitForFixedUpdate();
        }

        m_states.m_gravityControllState = GravityState.GravityEnabled;
        m_isSpeedBoosting = false;

        m_speedBoostCoroutine = StartCoroutine(ChargeSpeedBoost());
    }

    public void OnUppercutInputDown()
    {
        if (!m_isUppercutting)
        {
            StartCoroutine(RunUppercut());
        }
    }

    private IEnumerator RunUppercut()
    {
        m_isUppercutting = true;

        m_states.m_movementControllState = MovementControllState.MovementDisabled;
        m_states.m_gravityControllState = GravityState.GravityDisabled;

        float t = 0;

        Vector3 startPos = transform.position;

        while (t < m_uppercutTime)
        {
            t += Time.fixedDeltaTime;

            float progress = m_uppercutCurve.Evaluate(t / m_uppercutTime);

            Vector3 targetPos = Vector3.Lerp(startPos, startPos + Vector3.up * m_uppercutDistance, progress);
            PhysicsSeekTo(targetPos);

            yield return new WaitForFixedUpdate();
        }

        m_states.m_movementControllState = MovementControllState.MovementEnabled;
        m_states.m_gravityControllState = GravityState.GravityEnabled;

        m_isUppercutting = false;
    }

    public void OnSlamInputDown()
    {
        if (!m_isSlaming)
        {
            StartCoroutine(RunSlam());
        }


    }

    private IEnumerator RunSlam()
    {
        m_isSlaming = true;

        m_states.m_movementControllState = MovementControllState.MovementDisabled;
        m_states.m_gravityControllState = GravityState.GravityDisabled;

        float t = 0;

        Vector3 slamTarget = Vector3.zero;
        Vector3 startPos = transform.position;

        RaycastHit hit;

        if (Physics.Raycast(m_camera.transform.position, m_camera.transform.forward, out hit, Mathf.Infinity, m_wallMask))
        {
            slamTarget = hit.point;
        }

        while (t < m_slamTime)
        {
            t += Time.fixedDeltaTime;

            float progress = m_slamCurve.Evaluate(t / m_slamTime);

            Vector3 targetPos = Vector3.Lerp(startPos, slamTarget, progress);
            PhysicsSeekTo(targetPos);

            yield return new WaitForFixedUpdate();
        }

        m_states.m_movementControllState = MovementControllState.MovementEnabled;
        m_states.m_gravityControllState = GravityState.GravityEnabled;

        m_isSlaming = false;
    }

    private PlayerController[] CheckHitbox(Vector3 p_hitboxOrigin, float p_hitboxSize, LayerMask p_layerMask)
    {
        DebugExtension.DebugWireSphere(p_hitboxOrigin, p_hitboxSize);

        Collider[] colliders = Physics.OverlapSphere(p_hitboxOrigin, p_hitboxSize, p_layerMask);

        List<PlayerController> playerList = new List<PlayerController>();

        foreach (Collider collider in colliders)
        {
            if (collider.transform.root != transform)
            {
                playerList.Add(collider.gameObject.GetComponentInParent<PlayerController>());
            }
        }

        return playerList.ToArray();
    }

    private void TriggerKnockBack(Vector3 p_forceDirection, float p_force)
    {
        m_velocity = p_forceDirection.normalized * p_force;
    }

    private void StopAllActions()
    {
        StopAllCoroutines();
    }

    public bool CheckCollisionLayer(LayerMask p_layerMask, GameObject p_object)
    {
        if (p_layerMask == (p_layerMask | (1 << p_object.layer)))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        m_punchHitNormal = hit.normal;
        m_punchHitPos = hit.point;
    }
}
