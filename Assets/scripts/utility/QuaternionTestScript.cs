using System.Collections;
﻿using UnityEngine;

public class QuaternionTestScript : MonoBehaviour
{
	public Vector3 RotationAmount = new Vector3(0.0f, 90.0f, 0.0f);

	public void Update()
	{
		if(Input.GetButtonDown("Fire1")) {
			Quaternion quat_rotaiton = Quaternion.Euler(RotationAmount);
			this.transform.localRotation = Quaternion.LookRotation(quat_rotaiton*this.transform.forward);
		}
	}
}
