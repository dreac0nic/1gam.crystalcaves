using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class FirstPersonMovementController : MonoBehaviour
{
	[Header("Camera Control")]
	public Transform CameraAnchor;
	public bool JoystickEnabled = true;
	public Vector2 Sensitivity = Vector2.one;
	public bool MouseAcceleration = false;
	public float AccelerationAmount = 1.0f;
	public bool LookSmoothing = false;
	public float SmoothingDuration = 8.5f;
	public bool ClampVerticalAngle = true;
	public float MinimumVerticalAngle = -89.0f;
	public float MaximumVerticalAngle = 89.0f;

	[Header("Movement")]
	public float MaximumVelocity = 15.0f;
	public float Acceleration = 20.0f;
	public float JumpForce = 10.0f;
	public float JumpLandingCooldown = 0.1f;

	[Header("Jump Ghosting")]
	public bool EnableGhosting = false;
	public float GhostingDuration = 0.5f;

	[Header("Ground Check")]
	public Transform GroundCheckStart;
	public float GroundCheckDistance = 0.01f;
	public float GroundCheckRadius = 0.5f;
	public Vector3 GroundCheckDirection = Vector3.down;

	[Header("Input Names")]
	public string MovementAxis = "Vertical";
	public string StrafingAxis = "Horizontal";
	public string MouseXAxis = "Mouse X";
	public string MouseYAxis = "Mouse Y";
	public string ControllerLookX = "Look Horizontal";
	public string ControllerLookY = "Look Vertical";
	public string Jump = "Jump";

	// External references for player components
	protected Rigidbody m_Rigidbody;
	protected CapsuleCollider m_Collider;

	// State Variables
	protected bool m_IsGrounded = false;
	protected bool m_JumpInput = false;
	protected bool m_Jumped = false;
	protected float m_LandingTimeout;
	protected float m_GhostingTimeout;
	protected Vector2 m_MovementInput;
	protected Vector3 m_GroundNormal;

	public void Awake()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
		m_Collider = GetComponent<CapsuleCollider>();

		if(!CameraAnchor && Debug.isDebugBuild) {
			Debug.LogWarning("FirstPersonMovementController: Camera Anchor has not been assigned. Looking will be disabled until an anchor is assigned.");
		}

		if(!GroundCheckStart) {
			GroundCheckStart = this.transform;

			if(Debug.isDebugBuild) {
				Debug.LogWarning("FirstPersonMovementController: Ground Check Start has not been assigned. Reassigning to object transform.");
			}
		}
	}

	public void Update()
	{
		// Update state inputs
		m_JumpInput = Input.GetButton(Jump) && (Time.time >= m_LandingTimeout);
		m_MovementInput = new Vector2(Input.GetAxisRaw(StrafingAxis), Input.GetAxisRaw(MovementAxis));
		Vector3 rotational_input = new Vector2(Input.GetAxisRaw(MouseXAxis), Input.GetAxisRaw(MouseYAxis));

		if(CameraAnchor) {
			Quaternion camera_rotation = CameraAnchor.localRotation;
			Quaternion character_rotation = this.transform.localRotation;

			if(MouseAcceleration) {
				rotational_input.x *= AccelerationAmount*Mathf.Abs(rotational_input.x);
				rotational_input.y *= AccelerationAmount*Mathf.Abs(rotational_input.y);
			}

			if(JoystickEnabled) {
				rotational_input.x -= Input.GetAxisRaw(ControllerLookX);
				rotational_input.y += Input.GetAxisRaw(ControllerLookY);
			}

			if(rotational_input.magnitude > float.Epsilon) {
				rotational_input = Vector3.Scale(rotational_input, Sensitivity);

				camera_rotation *= Quaternion.Euler(-rotational_input.y, 0.0f, 0.0f);
				character_rotation *= Quaternion.Euler(0.0f, rotational_input.x, 0.0f);
			}

			if(ClampVerticalAngle) {
				camera_rotation = clampRotationAroundXAxis(camera_rotation, MinimumVerticalAngle, MaximumVerticalAngle);
			}

			if(LookSmoothing) {
				CameraAnchor.localRotation = Quaternion.Slerp(CameraAnchor.localRotation, camera_rotation, SmoothingDuration*Time.deltaTime);
				this.transform.localRotation = Quaternion.Slerp(this.transform.localRotation, character_rotation, SmoothingDuration*Time.deltaTime);
			} else {
				CameraAnchor.localRotation = camera_rotation;
				this.transform.localRotation = character_rotation;
			}
		}
	}

	public void FixedUpdate()
	{
		RaycastHit hit_info;
		Vector3 look_forward = (CameraAnchor ? CameraAnchor.forward : this.transform.forward);
		Vector3 look_right = (CameraAnchor ? CameraAnchor.right : this.transform.right);

		// Perform ground check to see if we have left the ground.
		if(Physics.SphereCast(GroundCheckStart.position, GroundCheckRadius, GroundCheckDirection, out hit_info, (0.5f*m_Collider.height - GroundCheckRadius) + GroundCheckDistance)) {
			if(!m_IsGrounded) {
				m_IsGrounded = true;
				m_LandingTimeout = Time.time + JumpLandingCooldown;
			}

			m_Jumped = false;
			m_GroundNormal = hit_info.normal;
		} else if(m_IsGrounded) {
			m_IsGrounded = false;
			m_GroundNormal = Vector3.up;
			m_GhostingTimeout = Time.time + GhostingDuration;
		}

		// Jump!
		if(m_JumpInput && !m_Jumped && (m_IsGrounded || (EnableGhosting && Time.time < m_GhostingTimeout))) {
			m_Jumped = true;

			// XXX: If we're in air due to ghosting, cancel out the downwards force for a single frame.
			if(!m_IsGrounded) {
				Vector3 rb_velocity = m_Rigidbody.velocity;
				rb_velocity.y = 0.0f;

				m_Rigidbody.velocity = rb_velocity;
			}

			m_Rigidbody.AddForce(JumpForce*Vector3.up, ForceMode.Impulse);
		}

		// Calculate movement based on current inputs
		if(m_MovementInput.magnitude > float.Epsilon && m_IsGrounded) {
			float time_acceleration = Acceleration*Time.deltaTime;
			Vector3 movement_input = (m_MovementInput.y*look_forward + m_MovementInput.x*look_right);
			movement_input = Vector3.ClampMagnitude(movement_input, 1.0f);

			Vector3 target_velocity = MaximumVelocity*Vector3.ProjectOnPlane(movement_input, m_GroundNormal).normalized;
			Vector3 velocity_delta = target_velocity - m_Rigidbody.velocity;

			velocity_delta = Mathf.Clamp(velocity_delta.magnitude, -time_acceleration, time_acceleration)*velocity_delta.normalized;

			m_Rigidbody.AddForce(velocity_delta, ForceMode.VelocityChange);
		}
	}

	private Quaternion clampRotationAroundXAxis(Quaternion rotation, float min, float max)
	{
		float angle;

		rotation.x /= rotation.w;
		rotation.y /= rotation.w;
		rotation.z /= rotation.w;
		rotation.w = 1.0f;

		angle = 2.0f*Mathf.Rad2Deg*Mathf.Atan(rotation.x);
		angle = Mathf.Clamp(angle, min, max);
		rotation.x = Mathf.Tan(0.5f*Mathf.Deg2Rad*angle);

		return rotation;
	}
}
