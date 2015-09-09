using UnityEngine;
using System.Collections;

public class InteractableUsable : Interactable
{
	public bool AllowAnonymouseTriggering = true;

	public void Trigger(GameObject offender = null)
	{
		if(AllowAnonymouseTriggering || offender) {
			processInteraction(offender);
		}
	}
}
