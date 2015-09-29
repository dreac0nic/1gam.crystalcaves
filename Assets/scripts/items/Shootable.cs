using System.Collections;
﻿using UnityEngine;

public class Shootable : MonoBehaviour
{
	public enum BlendingMode { NONE, SET, ADD, MULTIPLY };
	public enum WeaponState { PRIMING, READY, RELOADING, FIRING };

	[Header("Firing Mechanics")]
	[SerializeField] protected Transform Barrel;
	public float CycleTime = 0.8f;
	public int ProjectilesPerShot = 1;
	public float SpreadAngle = 3.0f;

	[Header("Projectile")]
	public GameObject ProjectilePrefab;
	public float ProjectileForce = 25.0f;
	public ForceMode ProjectileForceMode = ForceMode.Impulse;
	public float Damage = 10.0f;
	public BlendingMode DamageBlendStyle = BlendingMode.SET;
	public bool RaycastFalloffDamage = false;
	public float RaycastFalloffCloseDistance = 25.0f;
	public float RaycastFalloffFarDistance = 200.0f;
	public float RaycastFalloffFarDamage = 2.0f;
	public GameObject RaycastImpactPrefab;
	public float RaycastImpactDuration = 30.0f;

	[Header("Ammunition")]
	public int Ammo = 10;
	public int MagazineSize = 10;
	public bool ContinuousReload = true;
	public float ReloadDuration = 2.0f;

	protected WeaponState m_State = WeaponState.READY;
	protected float m_StateReleaseTimeout;

	public void Awake()
	{
		if(!Barrel) {
			Barrel = this.transform;
		}
	}

	public void Update()
	{
		if(Debug.isDebugBuild && Barrel) {
			Debug.DrawRay(Barrel.position, 50.0f*Barrel.forward, Color.green);
		}

		if(m_State != WeaponState.READY && Time.time >= m_StateReleaseTimeout) {
			switch(m_State) {
				case WeaponState.RELOADING:
					if(ContinuousReload) {
						Ammo++;
						m_StateReleaseTimeout = Time.time + ReloadDuration;
					} else {
						Ammo = MagazineSize;
					}

					if(Ammo >= MagazineSize) {
						Ammo = MagazineSize;
						m_State = WeaponState.READY;
					}
					break;

				default:
					m_State = WeaponState.READY;
					break;
			}
		}

		if(Input.GetButtonDown("Fire1") || Input.GetButton("Fire2")) {
			Debug.LogWarning("DEBUG FIRE REMOVE PLEASE");

			Fire();
		} else if(Input.GetButtonDown("Jump")) {
			Debug.LogWarning("DEBUG JUMP REMOVE PLEASE");

			Reload();
		}
	}

	public void Fire(Transform fire_point = null)
	{
		RaycastHit hit_info;

		if(!fire_point) {
			fire_point = Barrel;
		}

		if(m_State != WeaponState.FIRING && Ammo > 0) {
			if(Debug.isDebugBuild) {
				Debug.Log("Shootable: " + this.gameObject.name + " firing!");
			}

			for(int projectile_i = 0; projectile_i < ProjectilesPerShot; ++projectile_i) {
				float offset_spin = 360.0f*Random.value;
				float offset_tilt = SpreadAngle*(Random.value*Random.value*Random.value);
				Quaternion rotation_offset = fire_point.rotation*Quaternion.Inverse(Quaternion.Euler(new Vector3(0.0f, offset_tilt, offset_spin))); // FIXME: Do this with quaternions. Converting is really childish.
				Vector3 firing_direction = rotation_offset*Vector3.forward;

				Debug.Log(rotation_offset.eulerAngles);

				if(Debug.isDebugBuild) {
					Debug.DrawRay(fire_point.position, 50.0f*(firing_direction), new Color(0.8f, 0.4f, 0.0f), 1.2f);
				}

				if(!ProjectilePrefab) {
					if(Physics.Raycast(fire_point.position, firing_direction, out hit_info)) {
						float distance_damage = (RaycastFalloffDamage ? Damage - Mathf.InverseLerp(RaycastFalloffCloseDistance, RaycastFalloffFarDistance, hit_info.distance)*(Damage - RaycastFalloffFarDamage) : Damage);

						if(RaycastImpactPrefab) {
							GameObject impact = (GameObject)Instantiate(RaycastImpactPrefab, hit_info.point, Quaternion.FromToRotation(Vector3.up, hit_info.normal));
							impact.transform.localRotation = impact.transform.localRotation*Quaternion.Euler(new Vector3(0.0f, 360.0f*Random.value, 0.0f));
							impact.transform.SetParent(hit_info.collider.transform, true);

							Destroy(impact, RaycastImpactDuration);
						}
					}
				} else {
					GameObject projectile = (GameObject)Instantiate(ProjectilePrefab, fire_point.position, rotation_offset);
					Rigidbody projectile_body = projectile.GetComponent<Rigidbody>();

					if(projectile_body) {
						projectile_body.AddForce(ProjectileForce*projectile.transform.forward, ProjectileForceMode);
					}
				}
			}

			m_State = WeaponState.FIRING;
			m_StateReleaseTimeout = Time.time + CycleTime;
			Ammo = (Ammo <= 0 ? 0 : Ammo - 1);
		}
	}

	public void Reload()
	{
		if(m_State == WeaponState.READY && Ammo != MagazineSize) {
			m_State = WeaponState.RELOADING;
			m_StateReleaseTimeout = Time.time + ReloadDuration;
		}
	}
}
