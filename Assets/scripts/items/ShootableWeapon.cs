using System.Collections;
﻿using UnityEngine;

public class ShootableWeapon : MonoBehaviour
{
	[SerializeField] protected Transform Barrel;

	public abstract void Fire(Transform fire_point = null);
	public abstract void Reload();

	public virtual void Awake()
	{
		if(!Barrel) {
			Barrel = this.transform;
		}
	}

	public virtual void Update()
	{
		if(Debug.isDebugBuild && Barrel) {
			Debug.DrawRay(Barrel.position, 50.0f*Barrel.forward, Color.green);
		}
	}
}
