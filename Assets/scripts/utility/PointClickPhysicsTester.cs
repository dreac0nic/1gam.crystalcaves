using UnityEngine;
using System.Collections;

public class PointClickPhysicsTester : MonoBehaviour
{
	[Header("Object Throwing")]
	public GameObject ThrowPrefab;
	public float ThrowForce = 25.0f;
	public Vector3 SpawnOffset = 2.5f*Vector3.forward;

	[Header("Explosive Click")]
	public float ExplosionRadius = 5.0f;
	public float ExplosionPower = 10.0f;
	public float ExplosionUpwardForce = 2.5f;

	[Header("Input Names")]
	public string ThrowInput = "Fire1";
	public string ExplosionInput = "Fire2";

	protected bool m_ThrowTrigger;
	protected bool m_ExplosionTrigger;
	protected Vector3 m_MousePos;

	public void Update()
	{
		m_ThrowTrigger = Input.GetButtonDown(ThrowInput);
		m_ExplosionTrigger = Input.GetButtonDown(ExplosionInput);
		m_MousePos = Input.mousePosition;
	}

	public void FixedUpdate()
	{
		Ray target_ray = Camera.main.ScreenPointToRay(m_MousePos);

		if(m_ThrowTrigger) {
			if(ThrowPrefab) {
				GameObject bullet = (GameObject)Instantiate(ThrowPrefab, Camera.main.transform.position + Camera.main.transform.TransformDirection(SpawnOffset), Quaternion.LookRotation(target_ray.direction));
				Rigidbody body = bullet.GetComponent<Rigidbody>();

				body.AddForce(ThrowForce*bullet.transform.forward);
			}

			m_ThrowTrigger = false;
		}

		if(m_ExplosionTrigger) {
			RaycastHit hitinfo;

			if(Physics.Raycast(target_ray, out hitinfo)) {
				foreach(Collider victim in Physics.OverlapSphere(hitinfo.point, ExplosionRadius)) {
					Rigidbody body = victim.GetComponent<Rigidbody>();

					if(body) {
						body.AddExplosionForce(ExplosionPower, hitinfo.point, ExplosionRadius, ExplosionUpwardForce);
					}
				}
			}

			m_ExplosionTrigger = false;
		}
	}
}
