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
	protected Vector3 m_Velocity;
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
		if(m_Velocity.magnitude > float.Epsilon) {
			m_Velocity -= Friction*m_Velocity.normalized*Time.deltaTime;
		} else {
			m_Velocity = Vector3.zero;
		}

		if(m_MovementInput.magnitude > float.Epsilon && m_Rigidbody.velocity.magnitude < MaxSpeed) {
			m_Velocity += Acceleration*(new Vector3(m_MovementInput.x, 0.0f, m_MovementInput.y))*Time.deltaTime;

			if(m_Velocity.magnitude > MaxSpeed) {
				m_Velocity = m_Velocity.normalized*MaxSpeed;
			}

			m_Rigidbody.AddForce(m_Velocity, ForceMode.VelocityChange);
		}
	}
}
