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
    public enum AliveState { IsAlive, IsDead }
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
	[System.Serializable]
	public struct CameraProperties
	{
		public float m_mouseSensitivity;
		public float m_maxCameraAng;
		public bool m_inverted;
		public Camera m_camera;
		public Transform m_cameraTilt;
		public Transform m_cameraMain;
	}

	[Header("Camera Properties")]
	public CameraProperties m_cameraProperties;
	#endregion

	#region Base Movement Properties
	[System.Serializable]
	public struct BaseMovementProperties
	{
		public float m_baseMovementSpeed;
		public float m_accelerationTime;
	}

	[Header("Base Movement Properties")]
	public BaseMovementProperties m_baseMovementProperties;

    private float m_currentMovementSpeed;
    [HideInInspector]
    public Vector3 m_velocity;
    private Vector3 m_velocitySmoothing;
    private CharacterController m_characterController;
	private Coroutine m_jumpBufferCoroutine;
	private Coroutine m_graceBufferCoroutine;
	#endregion

	#region Jumping Properties
	[System.Serializable]
	public struct JumpingProperties
	{
		[Header("Jump Properties")]
		public float m_maxJumpHeight;
		public float m_minJumpHeight;
		public float m_timeToJumpApex;

		[Header("Jump Buffer Properties")]
		public float m_graceTime;
		public float m_jumpBufferTime;
	}

	[Header("Jumping Properties")]
	public JumpingProperties m_jumpingProperties;

    private float m_graceTimer;
    private float m_jumpBufferTimer;

    private float m_gravity;
    private float m_maxJumpVelocity;
    private float m_minJumpVelocity;
    private bool m_isLanded;
    private bool m_offLedge;
	#endregion

	#region Wall Run Properties
	[System.Serializable]
	public struct WallRunProperties
	{
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
	}

	[Header("Wall Run Properties")]
	public WallRunProperties m_wallRunProperties;

    private float m_currentWallRunningSpeed;

    private float m_wallRunBufferTimer;
    private float m_wallJumpBufferTimer;

    private float m_tiltTarget;
    private float m_tiltSmoothingVelocity;

    private bool m_isWallRunning;
    private bool m_connectedWithWall;
	[HideInInspector]
    public bool m_holdingWallRideStick;

    private Vector3 m_wallHitPos;
    private float m_wallHitDst;
    private Vector3 m_wallDir;
    private Vector3 m_wallVector;
    private Vector3 m_wallFacingVector;
    private Vector3 m_modelWallRunPos;

	private Coroutine m_wallJumpBufferCoroutine;
	private Coroutine m_wallRunBufferCoroutine;
	#endregion

	#region Wall Climb Properties
	[System.Serializable]
	public struct WallClimbProperties
	{
		public AnimationCurve m_wallClimbSpeedCurve;
		public float m_maxWallClimbSpeed;
		public float m_wallClimbSpeedUpTime;
		public float m_wallClimbFactor;
		public Vector3 m_wallClimbJumpVelocity;
	}

	[Header("Wall Climb Properties")]
	public WallClimbProperties m_wallClimbProperties;


    private float m_currentWallClimbSpeed;
    private bool m_isWallClimbing;
    [HideInInspector]
    public Vector3 m_localWallFacingVector;
	#endregion

	#region Punch Properties
	[System.Serializable]
	public struct PunchProperties
	{
		[Header("Punch Charge Properties")]
		public float m_punchChargeTime;

		[Header("Punch Attack Properties")]
		public float m_minPunchTime;
		public float m_maxPunchTime;
		public float m_punchSpeed;

		[Header("Punch Cooldown Properties")]
		public float m_punchCooldownTime;
		public PlayerUICooldown m_punchCooldownUI;
	}

	[Header("Punch Properties")]
	public LayerMask m_playerFistMask;
	public PunchProperties m_punchProperties;

	private float m_punchCooldownTimer;
	private bool m_isChargingPunch;
	private bool m_isPunching;
	private Coroutine m_punchCooldownCoroutine;
    #endregion

    #region Uppercut Properties
	[System.Serializable]
	public struct UppercutProperties
	{
		[Header("Uppercut Attack Properties")]
		public float m_uppercutTime;
		public float m_uppercutDistance;
		public AnimationCurve m_uppercutCurve;

		[Header("Uppercut Cooldown Properties")]
		public float m_uppercutCooldownTime;
		public PlayerUICooldown m_uppercutCooldownUI;

	}

	[Header("Uppercut Properties")]
	public UppercutProperties m_uppercutProperties;

	private bool m_isUppercutting;
	private Coroutine m_uppercutCoroutine;
	private float m_uppercutCooldownTimer;
	#endregion

	#region Short Dash Properties
	[System.Serializable]
	public struct ShortDashProperties
	{
		[Header("Short Dash Attack Properties")]
		public float m_shortDashTime;
		public float m_shortDashDistance;
		public AnimationCurve m_shortDashCurve;
	}

	[Header("Short Dash Properties")]
	public ShortDashProperties m_shortDashProperties;

    private bool m_isShortDashing;
    #endregion

    #region Speed Boost Properties
	[System.Serializable]
	public struct SpeedBoostProperties
	{
		[Header("Speed Boost Properties")]
		public float m_speedBoostChargeTime;
		public float m_speedBoostChargeMax;
		public float m_speedBoostTime;
		public float m_speedBoostSpeed;
		public AnimationCurve m_speedBoostCurve;
	}

	[Header("Speed Boost Properties")]
	public SpeedBoostProperties m_speedBoostProperties;

    private bool m_isSpeedBoosting;
	private float m_speedBoostChargeTimer;
	private float m_speedBoostCharge;
	private Coroutine m_speedBoostCoroutine;
    #endregion

    #region Slam Properties
	[System.Serializable]
	public struct SlamProperties
	{
		[Header("Slam Attack Properties")]
		public LayerMask m_slamWallMask;
		public float m_slamTime;
		public AnimationCurve m_slamCurve;
	}

	[Header("Slam Properties")]
	public SlamProperties m_slamProperties;

    private bool m_isSlaming;
	[Space]
	#endregion

	public LayerMask m_killZoneMask;

    private Vector2 m_movementInput;
    private Vector2 m_lookInput;

    private bool m_isStunned;

    private float m_currentSpeedBoost;

	private PlayerInput m_input;

    private void Start()
    {
        m_characterController = GetComponent<CharacterController>();

        CalculateJump();
        LockCursor();

        m_currentMovementSpeed = m_baseMovementProperties.m_baseMovementSpeed;
        m_jumpBufferTimer = m_jumpingProperties.m_jumpBufferTime;

        m_wallJumpBufferTimer = m_wallRunProperties.m_wallJumpBufferTime;
        m_wallRunBufferTimer = m_wallRunProperties.m_wallRunBufferTime;

        m_speedBoostChargeTimer = m_speedBoostProperties.m_speedBoostChargeTime;
        m_speedBoostCharge = m_speedBoostProperties.m_speedBoostChargeMax;

		m_punchCooldownTimer = m_punchProperties.m_punchCooldownTime;
		m_uppercutCooldownTimer = m_uppercutProperties.m_uppercutCooldownTime;
	}

	public void RunRoundSetup(int p_playerId, int p_numberOfPlayers)
	{
		m_input = GetComponent<PlayerInput>();
		m_input.m_playerId = p_playerId;

		#region Camera Size Calc
		if (p_numberOfPlayers == 1)
		{
			return;
		}
		else if (p_numberOfPlayers == 2)
		{
			Vector2 size = new Vector2(1, 0.5f);

			if (p_playerId == 0)
			{
				Vector2 pos = new Vector2(0,0);
				m_cameraProperties.m_camera.rect = new Rect(pos, size);
				return;
			}
			else if (p_playerId == 1)
			{
				Vector2 pos = new Vector2(0, 0.5f);
				m_cameraProperties.m_camera.rect = new Rect(pos, size);
				return;
			}
		}
		else if (p_numberOfPlayers == 3)
		{
			Vector2 size = new Vector2(0.5f, 0.5f);

			if (p_playerId == 0)
			{
				Vector2 pos = new Vector2(0, 0);
				m_cameraProperties.m_camera.rect = new Rect(pos, size);
				return;
			}
			else if (p_playerId == 1)
			{
				Vector2 pos = new Vector2(0.5f, 0);
				m_cameraProperties.m_camera.rect = new Rect(pos, size);
				return;
			}
			else if (p_playerId == 2)
			{
				Vector2 pos = new Vector2(0, 0.5f);
				m_cameraProperties.m_camera.rect = new Rect(pos, size);
				return;
			}
		}
		else if (p_numberOfPlayers == 4)
		{
			Vector2 size = new Vector2(0.5f, 0.5f);

			if (p_playerId == 0)
			{
				Vector2 pos = new Vector2(0, 0);
				m_cameraProperties.m_camera.rect = new Rect(pos, size);
				return;
			}
			else if (p_playerId == 1)
			{
				Vector2 pos = new Vector2(0.5f, 0);
				m_cameraProperties.m_camera.rect = new Rect(pos, size);
				return;
			}
			else if (p_playerId == 2)
			{
				Vector2 pos = new Vector2(0, 0.5f);
				m_cameraProperties.m_camera.rect = new Rect(pos, size);
				return;
			}
			else if (p_playerId == 3)
			{
				Vector2 pos = new Vector2(0.5f, 0.5f);
				m_cameraProperties.m_camera.rect = new Rect(pos, size);
				return;
			}
		}
		#endregion
	}

	private void OnValidate()
    {
        CalculateJump();
    }

    private void FixedUpdate()
    {
        PerformController();
    }

    public void PerformController()
    {
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
		m_cameraProperties.m_cameraMain.rotation = Quaternion.identity;
		m_cameraProperties.m_cameraTilt.rotation = Quaternion.identity;
    }

    private void CameraRotation()
    {
        //Get the inputs for the camera
        Vector2 cameraInput = new Vector2(m_lookInput.y * ((m_cameraProperties.m_inverted) ? -1 : 1), m_lookInput.x);

        if (!m_isPunching)
        {
            //Rotate the player on the y axis (left and right)
            transform.Rotate(Vector3.up, cameraInput.y * (m_cameraProperties.m_mouseSensitivity));
        }

        float cameraXAng = m_cameraProperties.m_cameraMain.transform.eulerAngles.x;

        if (!m_isPunching)
        {
            //Stops the camera from rotating, if it hits the resrictions
            if (cameraInput.x < 0 && cameraXAng > 360 - m_cameraProperties.m_maxCameraAng || cameraInput.x < 0 && cameraXAng < m_cameraProperties.m_maxCameraAng + 10)
            {
				m_cameraProperties.m_cameraMain.transform.Rotate(Vector3.right, cameraInput.x * (m_cameraProperties.m_mouseSensitivity));

            }
            else if (cameraInput.x > 0 && cameraXAng > 360 - m_cameraProperties.m_maxCameraAng - 10 || cameraInput.x > 0 && cameraXAng < m_cameraProperties.m_maxCameraAng)
            {
				m_cameraProperties.m_cameraMain.transform.Rotate(Vector3.right, cameraInput.x * (m_cameraProperties.m_mouseSensitivity));

            }

            if (m_cameraProperties.m_cameraMain.transform.eulerAngles.x < 360 - m_cameraProperties.m_maxCameraAng && m_cameraProperties.m_cameraMain.transform.eulerAngles.x > 180)
            {
				m_cameraProperties.m_cameraMain.transform.localEulerAngles = new Vector3(360 - m_cameraProperties.m_maxCameraAng, 0f, 0f);
            }
            else if (m_cameraProperties.m_camera.transform.eulerAngles.x > m_cameraProperties.m_maxCameraAng && m_cameraProperties.m_cameraMain.transform.eulerAngles.x < 180)
            {
				m_cameraProperties.m_cameraMain.transform.localEulerAngles = new Vector3(m_cameraProperties.m_maxCameraAng, 0f, 0f);
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

    //For UI Elements
	private IEnumerator RunBufferTimer(System.Action<float> m_bufferTimerRef, float p_bufferTime, PlayerUICooldown p_cooldownImage)
	{
		float t = 0;

		while (t < p_bufferTime)
		{
			t += Time.deltaTime;
			m_bufferTimerRef(t);
			p_cooldownImage.DisplayCooldown(t, p_bufferTime);
			yield return null;
		}

		m_bufferTimerRef(p_bufferTime);

		p_cooldownImage.DisplayCooldown(p_bufferTime, p_bufferTime);
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
		public AliveState m_aliveState;
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

		if (CheckBuffer(ref m_jumpBufferTimer, ref m_jumpingProperties.m_jumpBufferTime, m_jumpBufferCoroutine))
		{
			JumpMaxVelocity();
		}

        m_movementEvents.m_onLandedEvent.Invoke();
    }

    private void OnOffLedge()
    {
        m_offLedge = true;

        m_graceBufferCoroutine = StartCoroutine(RunBufferTimer((x) => m_graceTimer = (x), m_jumpingProperties.m_graceTime));

    }

    public void Respawn()
    {
        m_movementEvents.m_onRespawnEvent.Invoke();

        ResetCamera();
    }
    #endregion

    #region Physics Calculation Code

    private void CalculateCurrentSpeed()
    {
        float speed = m_baseMovementProperties.m_baseMovementSpeed;


        speed += m_currentWallRunningSpeed;
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
            Vector3 horizontalMovement = Vector3.SmoothDamp(m_velocity, targetHorizontalMovement, ref m_velocitySmoothing, m_baseMovementProperties.m_accelerationTime);

            m_velocity = new Vector3(horizontalMovement.x, m_velocity.y, horizontalMovement.z);
        }
        else
        {
            Vector2 input = new Vector2(0, 0);

            Vector3 forwardMovement = transform.forward * input.y;
            Vector3 rightMovement = transform.right * input.x;

            Vector3 targetHorizontalMovement = Vector3.ClampMagnitude(forwardMovement + rightMovement, 1.0f) * m_currentMovementSpeed;
            Vector3 horizontalMovement = Vector3.SmoothDamp(m_velocity, targetHorizontalMovement, ref m_velocitySmoothing, m_baseMovementProperties.m_accelerationTime);

            m_velocity = new Vector3(horizontalMovement.x, m_velocity.y, horizontalMovement.z);
        }

    }

    public void PhysicsSeekTo(Vector3 p_targetPosition)
    {
        Vector3 deltaPosition = p_targetPosition - transform.position;
        m_velocity = deltaPosition / Time.deltaTime;
    }
	#endregion

	#region Jump Code
	public void OnJumpInputDown()
	{
		m_jumpBufferCoroutine = StartCoroutine(RunBufferTimer((x) => m_jumpBufferTimer = (x), m_jumpingProperties.m_jumpBufferTime));

		if (CheckBuffer(ref m_wallJumpBufferTimer, ref m_wallRunProperties.m_wallJumpBufferTime, m_wallJumpBufferCoroutine) && !m_isWallRunning)
		{
			WallJump();
			return;
		}

		if (CheckBuffer(ref m_graceTimer, ref m_jumpingProperties.m_graceTime, m_graceBufferCoroutine) && !IsGrounded() && m_velocity.y <= 0f)
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
		m_gravity = -(2 * m_jumpingProperties.m_maxJumpHeight) / Mathf.Pow(m_jumpingProperties.m_timeToJumpApex, 2);
		m_maxJumpVelocity = Mathf.Abs(m_gravity) * m_jumpingProperties.m_timeToJumpApex;
		m_minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(m_gravity) * m_jumpingProperties.m_minJumpHeight);
	}

	private void WallJump()
	{
		m_movementEvents.m_onWallJumpEvent.Invoke();

		m_velocity.x = m_wallDir.x * m_wallRunProperties.m_wallJumpVelocity.x;
		m_velocity.y = m_wallRunProperties.m_wallJumpVelocity.y;
		m_velocity.z = m_wallDir.z * m_wallRunProperties.m_wallJumpVelocity.z;
	}

	private void WallRunningJump()
	{
		m_isWallRunning = false;

		m_movementEvents.m_onWallRunJumpEvent.Invoke();

		m_wallRunBufferCoroutine = StartCoroutine(RunBufferTimer((x) => m_wallRunBufferTimer = (x), m_wallRunProperties.m_wallRunBufferTime));

		m_velocity.x = m_wallDir.x * m_wallRunProperties.m_wallRunJumpVelocity.x;
		m_velocity.y = m_wallRunProperties.m_wallRunJumpVelocity.y;
		m_velocity.z = m_wallDir.z * m_wallRunProperties.m_wallRunJumpVelocity.z;
	}

	private void WallClimbingJump()
	{
		m_isWallClimbing = false;

		m_movementEvents.m_onWallClimbJumpEvent.Invoke();

		m_wallRunBufferCoroutine = StartCoroutine(RunBufferTimer((x) => m_wallRunBufferTimer = (x), m_wallRunProperties.m_wallRunBufferTime));

		m_velocity.x = m_wallDir.x * m_wallClimbProperties.m_wallClimbJumpVelocity.x;
		m_velocity.y = m_wallClimbProperties.m_wallClimbJumpVelocity.y;
		m_velocity.z = m_wallDir.z * m_wallClimbProperties.m_wallClimbJumpVelocity.z;
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
        float m_angleBetweenRays = m_wallRunProperties.m_wallRaySpacing / m_wallRunProperties.m_wallRidingRayCount;
        bool anyRayHit = false;

        for (int i = 0; i < m_wallRunProperties.m_wallRidingRayCount; i++)
        {
            Quaternion raySpaceQ = Quaternion.Euler(0, (i * m_angleBetweenRays) - (m_angleBetweenRays * (m_wallRunProperties.m_wallRidingRayCount / 2)), 0);
            RaycastHit hit;

            if (Physics.Raycast(m_characterController.transform.position, raySpaceQ * transform.forward, out hit, m_wallRunProperties.m_wallRunRayLength, m_wallRunProperties.m_wallMask))
            {
                if (Vector3.Dot(hit.normal, Vector3.up) == 0)
                {
                    anyRayHit = true;

                    m_wallVector = Vector3.Cross(hit.normal, Vector3.up);
                    m_wallFacingVector = Vector3.Cross(hit.normal, m_cameraProperties.m_camera.transform.forward);
                    m_wallDir = hit.normal;
                    m_wallHitPos = hit.point;
                    m_wallHitDst = hit.distance;

                    m_localWallFacingVector = m_cameraProperties.m_camera.transform.InverseTransformDirection(m_wallFacingVector);

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
        m_wallJumpBufferCoroutine = StartCoroutine(RunBufferTimer((x) => m_wallJumpBufferTimer = (x), m_wallRunProperties.m_wallJumpBufferTime));
    }

    private void TiltLerp()
    {
		m_cameraProperties.m_cameraTilt.localRotation = Quaternion.Slerp(m_cameraProperties.m_cameraTilt.localRotation, Quaternion.Euler(0, 0, m_tiltTarget), m_wallRunProperties.m_tiltSpeed);
    }

    private void OnWallRideRelease()
    {
        m_isWallRunning = false;
        m_isWallClimbing = false;
        m_wallRunBufferCoroutine = StartCoroutine(RunBufferTimer((x) => m_wallRunBufferTimer = (x), m_wallRunProperties.m_wallRunBufferTime));
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

            if (m_localWallFacingVector.x >= m_wallClimbProperties.m_wallClimbFactor)
            {
                if (!m_isWallClimbing)
                {
                    if (CheckOverBuffer(ref m_wallRunBufferTimer, ref m_wallRunProperties.m_wallRunBufferTime, m_wallRunBufferCoroutine))
                    {
                        StartCoroutine(WallClimbing());
                        return;
                    }
                }
            }

            if (!m_isWallRunning)
            {
                if (CheckOverBuffer(ref m_wallRunBufferTimer, ref m_wallRunProperties.m_wallRunBufferTime, m_wallRunBufferCoroutine))
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


            m_velocity = Vector3.zero;

            m_velocity.y = m_localWallFacingVector.x * m_currentMovementSpeed;

            float progress = m_wallClimbProperties.m_wallClimbSpeedCurve.Evaluate(t / m_wallClimbProperties.m_wallClimbSpeedUpTime);
            m_currentWallClimbSpeed = Mathf.Lerp(0f, m_wallClimbProperties.m_maxWallClimbSpeed, progress);

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


            float result = Mathf.Lerp(-m_wallRunProperties.m_wallRunCameraMaxTilt, m_wallRunProperties.m_wallRunCameraMaxTilt, m_wallFacingVector.y);
            m_tiltTarget = result;

            //m_modelRoot.localPosition = new Vector3(m_wallHitDst * Mathf.Sign(result), m_modelRoot.localPosition.y, m_modelRoot.localPosition.z);

            m_velocity = (m_wallVector * -m_wallFacingVector.y) * m_currentMovementSpeed;

            m_velocity += (transform.right * m_wallFacingVector.y) * m_currentMovementSpeed;

            m_velocity.y = 0;

            float progress = m_wallRunProperties.m_wallSpeedCurve.Evaluate(t / m_wallRunProperties.m_wallSpeedUpTime);
            m_currentWallRunningSpeed = Mathf.Lerp(0f, m_wallRunProperties.m_maxWallRunSpeed, progress);

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

	#region Attack Code

	#region Punch Code
	public void OnPunchInputDown()
    {
		if (CheckOverBuffer(ref m_punchCooldownTimer, ref m_punchProperties.m_punchCooldownTime, m_punchCooldownCoroutine))
		{
			if (!m_isChargingPunch)
			{
				StartCoroutine(ChargePunch());
			}
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
            yield return null;
        }

        StartCoroutine(RunPunch(m_cameraProperties.m_camera.transform.forward, t / m_punchProperties.m_punchChargeTime));

        m_isChargingPunch = false;
    }

    private IEnumerator RunPunch(Vector3 p_punchDirection, float p_punchChargePercent)
    {
        m_isPunching = true;
        m_states.m_movementControllState = MovementControllState.MovementDisabled;
        m_states.m_gravityControllState = GravityState.GravityDisabled;

        float currentPunchTime = Mathf.Lerp(m_punchProperties.m_minPunchTime, m_punchProperties.m_maxPunchTime, p_punchChargePercent);

        float t = 0;

        Vector3 startPos = transform.position;
		List<PlayerController> hitEnemies = new List<PlayerController>();

        while (t < currentPunchTime)
        {
            t += Time.fixedDeltaTime;

			m_velocity = p_punchDirection * m_punchProperties.m_punchSpeed;

			HitEnemies(ref hitEnemies, m_cameraProperties.m_camera.transform.forward, 250f, (m_cameraProperties.m_camera.transform.forward * 1) + transform.position, 1, m_playerFistMask);

            yield return new WaitForFixedUpdate();
        }

        m_states.m_movementControllState = MovementControllState.MovementEnabled;
        m_states.m_gravityControllState = GravityState.GravityEnabled;
        m_isPunching = false;

		m_punchCooldownCoroutine = StartCoroutine(RunBufferTimer((x) => m_punchCooldownTimer = (x), m_punchProperties.m_punchCooldownTime, m_punchProperties.m_punchCooldownUI));
	}
	#endregion

	#region Short Dash Code
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

        Vector3 dashPos = startPos + (p_dashDirection * m_shortDashProperties.m_shortDashDistance);

		List<PlayerController> hitEnemies = new List<PlayerController>();

		while (t < m_shortDashProperties.m_shortDashTime)
        {
            t += Time.fixedDeltaTime;

            float progress = m_shortDashProperties.m_shortDashCurve.Evaluate(t / m_shortDashProperties.m_shortDashTime);
            Vector3 targetPos = Vector3.Lerp(startPos, dashPos, progress);
            PhysicsSeekTo(targetPos);

			HitEnemies(ref hitEnemies, p_dashDirection, 350f, (transform.forward * 2) + transform.position, 2, m_playerFistMask);

			yield return new WaitForFixedUpdate();
        }

        m_states.m_movementControllState = MovementControllState.MovementEnabled;
        m_states.m_gravityControllState = GravityState.GravityEnabled;

        m_isShortDashing = false;
    }
	#endregion

	#region Speed Boost
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
        while (m_speedBoostCharge < m_speedBoostProperties.m_speedBoostChargeMax)
        {
            m_speedBoostChargeTimer += Time.deltaTime;

            float progress = m_speedBoostChargeTimer / m_speedBoostProperties.m_speedBoostChargeTime;

            m_speedBoostCharge = Mathf.Lerp(0, m_speedBoostProperties.m_speedBoostChargeMax, progress);

            yield return new WaitForFixedUpdate();
        }

        m_speedBoostCharge = m_speedBoostProperties.m_speedBoostChargeMax;
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
            float chargeProgress = m_speedBoostChargeTimer / m_speedBoostProperties.m_speedBoostChargeTime;
            m_speedBoostCharge = Mathf.Lerp(0f, m_speedBoostProperties.m_speedBoostChargeMax, chargeProgress);

            t += Time.fixedDeltaTime;
            float progress = m_speedBoostProperties.m_speedBoostCurve.Evaluate(t / m_speedBoostProperties.m_speedBoostTime);
            float currentSpeed = Mathf.Lerp(m_speedBoostProperties.m_speedBoostSpeed, m_speedBoostProperties.m_speedBoostSpeed, progress);

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
	#endregion

	#region Uppercut Code
	public void OnUppercutInputDown()
    {
		if (CheckOverBuffer(ref m_uppercutCooldownTimer, ref m_uppercutProperties.m_uppercutCooldownTime, m_uppercutCoroutine))
		{
			if (!m_isUppercutting)
			{
				StartCoroutine(RunUppercut());
			}
		}
    }

    private IEnumerator RunUppercut()
    {
        m_isUppercutting = true;

        m_states.m_movementControllState = MovementControllState.MovementDisabled;
        m_states.m_gravityControllState = GravityState.GravityDisabled;

        float t = 0;

        Vector3 startPos = transform.position;

		List<PlayerController> hitEnemies = new List<PlayerController>();

		while (t < m_uppercutProperties.m_uppercutTime)
        {
            t += Time.fixedDeltaTime;

            float progress = m_uppercutProperties.m_uppercutCurve.Evaluate(t / m_uppercutProperties.m_uppercutTime);

            Vector3 targetPos = Vector3.Lerp(startPos, startPos + Vector3.up * m_uppercutProperties.m_uppercutDistance, progress);
            PhysicsSeekTo(targetPos);

			HitEnemies(ref hitEnemies, transform.up, 50f, (transform.forward * 3) + transform.position, 3, m_playerFistMask);

			yield return new WaitForFixedUpdate();
        }

        m_states.m_movementControllState = MovementControllState.MovementEnabled;
        m_states.m_gravityControllState = GravityState.GravityEnabled;

        m_isUppercutting = false;

		m_uppercutCoroutine = StartCoroutine(RunBufferTimer((x) => m_uppercutCooldownTimer = (x), m_uppercutProperties.m_uppercutCooldownTime, m_uppercutProperties.m_uppercutCooldownUI));
	}
	#endregion

	#region Slam Code
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

        if (Physics.Raycast(m_cameraProperties.m_camera.transform.position, m_cameraProperties.m_camera.transform.forward, out hit, Mathf.Infinity, m_slamProperties.m_slamWallMask))
        {
            slamTarget = hit.point;
        }

        while (t < m_slamProperties.m_slamTime)
        {
            t += Time.fixedDeltaTime;

            float progress = m_slamProperties.m_slamCurve.Evaluate(t / m_slamProperties.m_slamTime);

            Vector3 targetPos = Vector3.Lerp(startPos, slamTarget, progress);
            PhysicsSeekTo(targetPos);

            yield return new WaitForFixedUpdate();
        }

        m_states.m_movementControllState = MovementControllState.MovementEnabled;
        m_states.m_gravityControllState = GravityState.GravityEnabled;

        m_isSlaming = false;
    }
	#endregion

	private void HitEnemies(ref List<PlayerController> p_previouslyHitEnemies, Vector3 p_hitDirection, float p_hitForce, Vector3 p_hitboxOrigin, float p_hitboxSize, LayerMask p_layerMask)
	{
		PlayerController[] hitPlayers = CheckHitbox(p_hitboxOrigin, p_hitboxSize, p_layerMask);

		foreach (PlayerController player in hitPlayers)
		{
			if (!p_previouslyHitEnemies.Contains(player))
			{
				player.TriggerKnockBack(p_hitDirection, p_hitForce);
				p_previouslyHitEnemies.Add(player);
			}
		}
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
	#endregion

	public void FreezeSelf()
	{
		m_states.m_movementControllState = MovementControllState.MovementDisabled;
	}

	public void UnFreezeSelf()
	{
		m_states.m_movementControllState = MovementControllState.MovementEnabled;
	}

	private void KillSelf()
	{
		m_states.m_movementControllState = MovementControllState.MovementDisabled;
		m_states.m_aliveState = AliveState.IsDead;

        gameObject.SetActive(false);

		//m_cameraProperties.m_camera.enabled = false;

		RoundManager.Instance.OnPlayerDeath();
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
		if (CheckCollisionLayer(m_killZoneMask, hit.gameObject))
		{
			KillSelf();
		}
    }
}
