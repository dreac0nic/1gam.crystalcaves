using UnityEngine;
using System.Collections;

public class InteractableOnTrigger : Interactable
{
	public bool TriggerEnter = false;
	public bool TriggerStay = false;
	public bool TriggerExit = false;

	public virtual void OnTriggerEnter(Collider offender_collider)
	{
		if(TriggerEnter) {
			processInteraction(processOffender(offender_collider), FireTargetBase.TriggerType.ENTER);
		}
	}

	public virtual void OnTriggerStay(Collider offender_collider)
	{
		if(TriggerStay) {
			processInteraction(processOffender(offender_collider), FireTargetBase.TriggerType.STAY);
		}
	}

	public virtual void OnTriggerExit(Collider offender_collider)
	{
		if(TriggerExit) {
			processInteraction(processOffender(offender_collider), FireTargetBase.TriggerType.EXIT);
		}
	}

	protected virtual GameObject processOffender(Collider offender_collider)
	{
		return offender_collider.gameObject;
	}
}
