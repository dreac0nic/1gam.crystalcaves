using System.Collections;
﻿using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleRBMovement : MonoBehaviour
{
	public float MaxSpeed = 15.0f;
	public float Acceleration = 5.0f;
	public float Friction = 2.5f;

	public string HorizontalInput = "Horizontal";
	public string VerticalInput = "Vertical";

	protected Rigidbody m_Rigidbody;
	protected Vector2 m_MovementInput;

	public void Awake()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
	}

	public void Update()
	{
		m_MovementInput = new Vector2(Input.GetAxisRaw(HorizontalInput), Input.GetAxisRaw(VerticalInput));
	}

	public void FixedUpdate()
	{
		if(m_MovementInput.magnitude > float.Epsilon) {
			Vector3 target_velocity = MaxSpeed*this.transform.TransformDirection(new Vector3(m_MovementInput.x, 0.0f, m_MovementInput.y));
			Vector3 velocity_delta = target_velocity - m_Rigidbody.velocity;

			velocity_delta.x = Mathf.Clamp(velocity_delta.x, -Acceleration*Time.deltaTime, Acceleration*Time.deltaTime);
			velocity_delta.z = Mathf.Clamp(velocity_delta.z, -Acceleration*Time.deltaTime, Acceleration*Time.deltaTime);
			velocity_delta.y = 0.0f;

			m_Rigidbody.AddForce(velocity_delta, ForceMode.VelocityChange);
		}
	}
}
