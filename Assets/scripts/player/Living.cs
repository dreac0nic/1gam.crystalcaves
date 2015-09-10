using System.Collections;
﻿using UnityEngine;

public class Living : MonoBehaviour
{
	public float Health {
		get { return m_Health; }
	}

	public float HealthRatio {
		get { return m_Health/m_MaxHealth; }
	}

	public float MaxHealth {
		get { return m_MaxHealth; }
	}

	public bool IsAlive {
		get { return m_Health <= 0.0f; }
	}

	[SerializeField]
	protected float m_Health = 100.0f;

	[SerializeField]
	protected float m_MaxHealth = 100.0f;

	public virtual void TakeDamage(float damage)
	{
		m_Health -= damage;
	}

	public virtual void Resurrect(float health = float.NegativeInfinity)
	{
		if(health == float.NegativeInfinity) {
			m_Health = m_MaxHealth;
		} else {
			m_Health = health;
		}
	}
}
