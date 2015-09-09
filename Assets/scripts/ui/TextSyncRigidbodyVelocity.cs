using System.Collections;
﻿using UnityEngine;
﻿using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TextSyncRigidbodyVelocity : MonoBehaviour
{
	public Rigidbody Target;
	public string Format = "0.00";

	protected Text m_Text;

	public void Awake()
	{
		m_Text = GetComponent<Text>();
	}

	void FixedUpdate()
	{
		float current_velocity = 0.0f;

		if(Target) {
			Vector3 local_speed = Target.transform.InverseTransformDirection(Target.velocity);
			local_speed.y = 0;

			current_velocity = Mathf.Abs(local_speed.magnitude);
		}

		if(m_Text) {
			m_Text.text = string.Format("{0:" + (string.IsNullOrEmpty(Format) ? "0.00" : Format) + "}", current_velocity);
		} else {
			m_Text = GetComponent<Text>();
		}
	}
}
