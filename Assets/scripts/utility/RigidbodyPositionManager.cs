using System.Collections;
﻿using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyPositionManager : MonoBehaviour
{
	public Vector3 Velocity = -9.81f*Vector3.up;

	protected Rigidbody m_Rigidbody;
	protected Vector3 m_Position;

	public void Awake()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
	}

	public void Start()
	{
		m_Position = this.transform.position;
	}

	public void Update()
	{
		m_Position = this.transform.position + Velocity*Time.deltaTime;
	}

	public void FixedUpdate()
	{
		m_Rigidbody.MovePosition(m_Position);
	}
}
