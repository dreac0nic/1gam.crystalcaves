using UnityEngine;
using System.Collections;

public class FPSInteractor : MonoBehaviour {
	public Canvas InteractionOverlayPrefab;

	public bool InteractionSphereCast = false;
	public float InteractionRadius = 5.0f;
	public float InteractionDistance = 2.0f;

	public Camera PlayerCamera;
	private Canvas m_CurrentInteractionOverlay;

	void Start()
	{
		if(Debug.isDebugBuild)
			Debug.Log("FPSInteractor: [INFO] Initializing... PLEASE REFACTOR ME.");

		if(!PlayerCamera) {
			PlayerCamera = GetComponent<Camera>();

			if(Debug.isDebugBuild && !PlayerCamera)
				Debug.Log("FPSInteractor: [ERROR] No camera was found for Interactor. Interactor disabled until one is found.");
		}
	}

	void Update()
	{
		bool cleanup = false;
		Vector3 screenMidVector;
		RaycastHit hitInfo;

		// Skip if there is no camera reference.
		if(!PlayerCamera)
			return;

		// Update mid vector.
		screenMidVector = new Vector3(PlayerCamera.pixelWidth/2.0f, PlayerCamera.pixelHeight/2.0f, 0.0f);

		// Check for interactables within range.
		if(Physics.Raycast(PlayerCamera.ScreenPointToRay(screenMidVector), out hitInfo, InteractionDistance)) {
			Interactable interactee = hitInfo.collider.GetComponent<Interactable>();

			if(interactee) {
				// Interaction cue.
				if(interactee.InteractionCue && !m_CurrentInteractionOverlay) {
					m_CurrentInteractionOverlay = Instantiate(InteractionOverlayPrefab);
					m_CurrentInteractionOverlay.transform.SetParent(interactee.InteractionCueAnchor, false);
				}

				// Interact with object if interact button is pushed.
				if(Input.GetButtonDown("Interact"))
					interactee.Trigger(this.gameObject);
			} else
				cleanup = true;
		} else
			cleanup = true; // FIXME: Like really?

		// Cleanup canvas overlay
		if(cleanup && m_CurrentInteractionOverlay) {
			Destroy(m_CurrentInteractionOverlay.gameObject);

			m_CurrentInteractionOverlay = null;
		}

		// Debugging code!
		if(Debug.isDebugBuild)
			Debug.DrawRay(PlayerCamera.transform.position, PlayerCamera.ScreenPointToRay(screenMidVector).direction*InteractionDistance, Color.red);
	}
}
