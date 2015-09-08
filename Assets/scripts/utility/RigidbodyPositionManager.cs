using System.Collections;
﻿using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyPositionManager : MonoBehaviour
{
	public Vector3 StartVelocity = Vector3.zero;
	public Vector3 StartAcceleration = -9.81f*Vector3.up;

	protected Rigidbody m_Rigidbody;
	protected Vector3 m_Velocity;
	protected Vector3 m_Acceleration;

	public void Awake()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
	}

	public void Start()
	{
		m_Velocity = StartVelocity;
		m_Acceleration = StartAcceleration;
	}

	public void FixedUpdate()
	{
		m_Velocity += m_Acceleration*Time.deltaTime;
		m_Rigidbody.MovePosition(this.transform.position + m_Velocity*Time.deltaTime);
	}
}
