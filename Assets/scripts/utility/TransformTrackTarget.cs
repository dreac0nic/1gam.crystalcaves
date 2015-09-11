using System.Collections;
﻿using UnityEngine;

public class TransformTrackTarget : MonoBehaviour
{
	public Transform Target;
	public float FollowSpeed = 5.0f;
	
	public void Update()
	{
		if(Target) {
			this.transform.localRotation = Quaternion.Slerp(this.transform.localRotation, Quaternion.LookRotation(Target.position - this.transform.position), FollowSpeed*Time.deltaTime);
		}
	}
}
