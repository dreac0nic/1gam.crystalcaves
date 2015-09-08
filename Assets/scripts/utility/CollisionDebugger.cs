using UnityEngine;
using System.Collections;

public class CollisionDebugger : MonoBehaviour
{
	public string Tag = "";
	public bool DisplayDebugMessages = true;

	[Header("On Collision Enter")]
	public bool DrawRaysOnCollisionEnter = true;
	public bool RayDepthTestOnCollisionEnter = true;
	public float RayLifetimeOnCollisionEnter = 2.5f;
	public Color RayColorOnCollisionEnter = Color.red;
	public float RayLengthOnCollisionEnter = 5.0f;
	public bool ChangeColorOnCollisionEnter = true;
	public Color MaterialColorOnCollisionEnter = Color.red;

	[Header("On Collision Stay")]
	public bool DrawRaysOnCollisionStay = true;
	public bool RayDepthTestOnCollisionStay = true;
	public float RayLifetimeOnCollisionStay = 2.5f;
	public Color RayColorOnCollisionStay = Color.green;
	public float RayLengthOnCollisionStay = 5.0f;
	public bool ChangeColorOnCollisionStay = true;
	public Color MaterialColorOnCollisionStay = Color.green;

	[Header("On Collision Exit")]
	public bool DrawRaysOnCollisionExit = true;
	public bool RayDepthTestOnCollisionExit = true;
	public float RayLifetimeOnCollisionExit = 2.5f;
	public Color RayColorOnCollisionExit = Color.blue;
	public float RayLengthOnCollisionExit = 5.0f;
	public bool ChangeColorOnCollisionExit = true;
	public Color MaterialColorOnCollisionExit = Color.blue;

	public void OnCollisionEnter(Collision impact)
	{
		if(DisplayDebugMessages && Debug.isDebugBuild) {
			Debug.Log("<" + this.gameObject.name + "> has entered a collision with <" + impact.gameObject.name + ">");
		}

		handleCollision(impact, DrawRaysOnCollisionEnter, ChangeColorOnCollisionEnter, RayColorOnCollisionEnter, RayLifetimeOnCollisionEnter, RayLengthOnCollisionEnter, RayDepthTestOnCollisionEnter, MaterialColorOnCollisionEnter);
	}

	public void OnCollisionStay(Collision impact)
	{
		if(DisplayDebugMessages && Debug.isDebugBuild) {
			Debug.Log("<" + this.gameObject.name + "> has entered a collision with <" + impact.gameObject.name + ">");
		}

		handleCollision(impact, DrawRaysOnCollisionStay, ChangeColorOnCollisionStay, RayColorOnCollisionStay, RayLifetimeOnCollisionStay, RayLengthOnCollisionStay, RayDepthTestOnCollisionStay, MaterialColorOnCollisionStay);
	}

	public void OnCollisionExit(Collision impact)
	{
		if(DisplayDebugMessages && Debug.isDebugBuild) {
			Debug.Log("<" + this.gameObject.name + "> has entered a collision with <" + impact.gameObject.name + ">");
		}

		handleCollision(impact, DrawRaysOnCollisionExit, ChangeColorOnCollisionExit, RayColorOnCollisionExit, RayLifetimeOnCollisionExit, RayLengthOnCollisionExit, RayDepthTestOnCollisionExit, MaterialColorOnCollisionExit);
	}

	protected void handleCollision(Collision impact, bool draw_rays = true, bool change_color = true, Color ray_color = default(Color), float ray_lifetime = 2.5f, float ray_length = 5.0f, bool ray_depth = false, Color mat_color = default(Color))
	{
		foreach(ContactPoint contact in impact.contacts) {
			if(draw_rays) {
				Debug.DrawRay(contact.point, contact.normal*ray_length, ray_color, ray_lifetime, ray_depth);
			}

			if(change_color) {
				foreach(Renderer rend in GetComponentsInChildren<Renderer>()) {
					rend.material.color = mat_color;
				}
			}
		}
	}
}
