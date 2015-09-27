using UnityEngine;
using System.Collections;

public class FirstPersonUser : MonoBehaviour
{
	public Camera UserCamera;
	public float InteractionDistance = 2.5f;
	public string UseInput = "Interact";

	public void Awake()
	{
		if(!UserCamera) {
			UserCamera = Camera.main;

			if(Debug.isDebugBuild) {
				Debug.LogWarning("FPSUser: No Camera assigned, assigning to main camera: " + Camera.main.gameObject.name);
			}
		}
	}

	void Update()
	{
		Vector3 mid_vector = new Vector3(UserCamera.pixelWidth/2.0f, UserCamera.pixelHeight/2.0f, 0.0f);

		if(UserCamera) {
			RaycastHit hit_info;

			if(Physics.Raycast(UserCamera.ScreenPointToRay(mid_vector), out hit_info, InteractionDistance)) {
				InteractableUsable interactee = hit_info.collider.GetComponent<InteractableUsable>();

				if(interactee && Input.GetButtonDown(UseInput)) {
					interactee.Trigger(this.gameObject);
				}
			}
		}

		if(Debug.isDebugBuild) {
			Debug.DrawRay(UserCamera.transform.position, InteractionDistance*UserCamera.ScreenPointToRay(mid_vector).direction, Color.red);
		}
	}
}
