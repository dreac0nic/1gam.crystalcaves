using System.Collections;
﻿using UnityEngine;

[RequireComponent(typeof (Rigidbody))]
[RequireComponent(typeof (CapsuleCollider))]
public class FirstPersonMovementController : MonoBehaviour
{
	[Header("Camera Control")]
	public Transform CameraAnchor;
	public bool JoystickEnabled = true;
	public Vector2 Sensitivity = Vector2.one;
	public bool MouseAcceleration = false;
	public float AccelerationAmount = 1.0f;
	public bool LookSmoothing = false;
	public float SmoothingDuration = 10.0f;
	public bool ClampVerticalAngle = true;
	public float MinimumVerticalAngle = -89.0f;
	public float MaximumVerticalAngle = 89.0f;

	[Header("Movement")]
	public float MoveAcceleration = 150.0f;
	public float JumpForce = 50.0f;
	public float LandingRestPeriod = 0.1f;

	[Header("Air Control")]
	public bool AerialMovement = false;
	public bool ModifyDragDuringFreefall = false;
	public float FreefallDrag = 0.0f;

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
	public string LookHorizontalAxis = "Look Horizontal";
	public string LookVerticalAxis = "Look Vertical";
	public string Jump = "Jump";

	// External References
	private Rigidbody m_Rigidbody;
	private CapsuleCollider m_Collider;

	// State Values
	private Vector2 m_MovementInput;
	private bool m_JumpInput = false;

	// XXX: Does not allow for external rotation considering player target is never externally updated.
	private Quaternion m_TargetCamRotation;
	private Quaternion m_TargetCharacterRotation;

	private bool m_IsGrounded = true;
	private float m_RestOver = 0.0f;
	private float m_GroundDrag;
	private Vector3 m_GroundNormal = Vector3.up;

	public void Start()
	{
		m_Rigidbody = this.GetComponent<Rigidbody>();
		m_Collider = this.GetComponent<CapsuleCollider>();

		if(!GroundCheckStart) {
			GroundCheckStart = this.transform;
		}

		m_TargetCamRotation = (CameraAnchor ? CameraAnchor.localRotation : Quaternion.identity);
		m_TargetCharacterRotation = this.transform.localRotation;

		m_GroundDrag = m_Rigidbody.drag;

		if(!CameraAnchor && Debug.isDebugBuild) {
			Debug.LogWarning("FirstPersonMovementController: Camera Anchor has not been assigned. Looking disabled until an anchor is assigned.");
		}
	}

	public void Update()
	{
		Vector2 rotationalInput;

		m_MovementInput = new Vector2(Input.GetAxisRaw(StrafingAxis), Input.GetAxisRaw(MovementAxis));
		rotationalInput = new Vector2(Input.GetAxisRaw(MouseXAxis), Input.GetAxisRaw(MouseYAxis));

		if(CameraAnchor) {
			if(MouseAcceleration) {
				rotationalInput.x *= AccelerationAmount*Mathf.Abs(rotationalInput.x);
				rotationalInput.y *= AccelerationAmount*Mathf.Abs(rotationalInput.y);
			}

			if(JoystickEnabled) {
				rotationalInput.x -= Input.GetAxisRaw(LookHorizontalAxis);
				rotationalInput.y += Input.GetAxisRaw(LookVerticalAxis);
			}

			if(rotationalInput.magnitude > float.Epsilon) {
				rotationalInput = Vector3.Scale(rotationalInput, Sensitivity);

				m_TargetCamRotation *= Quaternion.Euler(-rotationalInput.y, 0.0f, 0.0f);
				m_TargetCharacterRotation *= Quaternion.Euler(0.0f, rotationalInput.x, 0.0f);
			}

			if(ClampVerticalAngle) {
				m_TargetCamRotation = clampRotationAroundXAxis(m_TargetCamRotation, MinimumVerticalAngle, MaximumVerticalAngle);
			}

			if(LookSmoothing) {
				CameraAnchor.localRotation = Quaternion.Slerp(CameraAnchor.localRotation, m_TargetCamRotation, SmoothingDuration*Time.deltaTime);
				this.transform.localRotation = Quaternion.Slerp(this.transform.localRotation, m_TargetCharacterRotation, SmoothingDuration*Time.deltaTime);
			} else {
				CameraAnchor.localRotation = m_TargetCamRotation;
				this.transform.localRotation = m_TargetCharacterRotation;
			}
		}

		m_JumpInput = Input.GetButton(Jump) && (Time.time >= m_RestOver);
	}

	public void FixedUpdate()
	{
		RaycastHit hit;
		Vector3 localForward = (CameraAnchor ? CameraAnchor.forward : this.transform.forward);
		Vector3 localRight = (CameraAnchor ? CameraAnchor.right : this.transform.right);

		// Check to see if the player is grounded.
		if(Physics.SphereCast(GroundCheckStart.transform.position, GroundCheckRadius, GroundCheckDirection, out hit, (0.5f*m_Collider.height - m_Collider.radius) + GroundCheckDistance)) {
			if(!m_IsGrounded) {
				m_IsGrounded = true;
				m_Rigidbody.drag = m_GroundDrag;
				m_RestOver = Time.time + LandingRestPeriod;
			}

			m_GroundNormal = hit.normal;
		} else if(m_IsGrounded) {
			m_IsGrounded = false;
			m_GroundNormal = Vector3.up;
			m_GroundDrag = m_Rigidbody.drag;

			if(ModifyDragDuringFreefall) {
				m_Rigidbody.drag = FreefallDrag;
			}
		}

		// Calculate movement
		if((m_IsGrounded || AerialMovement) && m_MovementInput.magnitude > float.Epsilon) {
			Vector3 cameraBiasedInput = localForward*m_MovementInput.y + localRight*m_MovementInput.x;
			Vector3 movement = Vector3.ProjectOnPlane(cameraBiasedInput, m_GroundNormal).normalized;

			m_Rigidbody.AddForce(movement*MoveAcceleration, ForceMode.Acceleration);
		}

		// Jump! This has to correct the ground check values to ensure that the proper drag is saved.
		if(m_IsGrounded && m_JumpInput) {
			m_IsGrounded = false;
			m_GroundDrag = m_Rigidbody.drag;

			if(ModifyDragDuringFreefall) {
				m_Rigidbody.drag = FreefallDrag;
			}

			m_Rigidbody.AddForce(Vector3.up*JumpForce, ForceMode.Impulse);
		}
	}

	// NOTE: Concepts taken form the Unity samples and modified for flexiblity. Thanks guys!
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
