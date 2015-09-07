using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof (Camera))]
public class PointClickInteractor : MonoBehaviour
{
	public Canvas InteractionOverlayPrefab;

	public float InteractionDistance = 2.0f;

	// TODO: Add reference to prefab to use as a cue for interacting with the object.

	private Camera m_Camera;
	private Canvas m_CurrentInteractionOverlay;

	void Start()
	{
		m_Camera = GetComponent<Camera>();
	}

	void Update()
	{
		bool cleanup = false;
		RaycastHit hitInfo;

		if(Physics.Raycast(m_Camera.ScreenPointToRay(Input.mousePosition), out hitInfo, InteractionDistance)) {
			Interactable interactee = hitInfo.collider.GetComponent<Interactable>();

			if(interactee) {
				// Pop interaction cue above interactable.
				if(interactee.InteractionCue && interactee.InteractionCueAnchor && !m_CurrentInteractionOverlay) {
					m_CurrentInteractionOverlay = Instantiate(InteractionOverlayPrefab);
					m_CurrentInteractionOverlay.transform.SetParent(interactee.InteractionCueAnchor, false);
				}

				// Interact with object if interact button is pushed.
				if(Input.GetButtonDown("Interact"))
					interactee.Trigger(this.gameObject);
			} else
				cleanup = true;
		} else
			cleanup = true;

		// Cleanup the canvas overlay.
		if(cleanup && m_CurrentInteractionOverlay) {
			Destroy(m_CurrentInteractionOverlay.gameObject);

			m_CurrentInteractionOverlay = null;
		}

		// Debugging code!
		if(Debug.isDebugBuild)
			Debug.DrawRay(m_Camera.transform.position, m_Camera.ScreenPointToRay(Input.mousePosition).direction*InteractionDistance, Color.red);
	}
}
