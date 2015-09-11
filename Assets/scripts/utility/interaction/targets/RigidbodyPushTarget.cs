using System.Collections;
﻿using UnityEngine;

public class RigidbodyPushTarget : FireTargetBase
{
	public float PushPower = 50.0f;

	public override void Fire(GameObject offender, TriggerType type = TriggerType.GENERAL)
	{
		Rigidbody body = offender.GetComponent<Rigidbody>();

		if(body) {
			body.AddForce(PushPower*this.transform.forward);
		}
	}
}
