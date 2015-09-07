using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof (Collider))]
public class Interactable : MonoBehaviour
{
	public bool InteractionCue;

	public Transform InteractionCueAnchor;

	public List<FireTargetBase> InteractionTargets;
	public List<FireTargetBase> TriggerEnterTargets;
	public List<FireTargetBase> TriggerStayTargets;
	public List<FireTargetBase> TriggerExitTargets;

	private StringBuilder m_DebugMessage;

	public void Start()
	{
		m_DebugMessage = new StringBuilder();

		if(!InteractionCueAnchor)
			InteractionCueAnchor = this.gameObject.transform;
	}

	public void Trigger(GameObject offender)
	{
		if(InteractionTargets.Count == 0)
			return;

		if(Debug.isDebugBuild) {
			m_DebugMessage.Length = 0; // .NET 4.0 anytime, Unity :)

			m_DebugMessage.Append("Interactable: [INFO] <");
			m_DebugMessage.Append(this.gameObject.transform.name);
			m_DebugMessage.Append("> was interacted with. Triggering listeners ... \n");
		}

		this.FireTargets(InteractionTargets, offender);

		if(Debug.isDebugBuild)
			Debug.Log(m_DebugMessage.ToString());
	}

	public void OnTriggerEnter(Collider offender)
	{
		if(TriggerEnterTargets.Count == 0)
			return;

		if(Debug.isDebugBuild) {
			m_DebugMessage.Length = 0;

			m_DebugMessage.Append("Interactable: [INFO] <");
			m_DebugMessage.Append(this.gameObject.transform.name);
			m_DebugMessage.Append("> had its trigger entered by <");
			m_DebugMessage.Append(offender.gameObject.transform.name);
			m_DebugMessage.Append(">. Triggering listeners ... \n");
		}

		this.FireTargets(TriggerEnterTargets, offender.gameObject, FireTargetBase.TriggerType.ENTER);

		if(Debug.isDebugBuild)
			Debug.Log(m_DebugMessage.ToString());
	}

	public void OnTriggerStay(Collider offender)
	{
		if(TriggerStayTargets.Count == 0)
			return;

		if(Debug.isDebugBuild) {
			m_DebugMessage.Length = 0;

			m_DebugMessage.Append("Interactable: [INFO] <");
			m_DebugMessage.Append(this.gameObject.transform.name);
			m_DebugMessage.Append("> had its trigger resided by <");
			m_DebugMessage.Append(offender.gameObject.transform.name);
			m_DebugMessage.Append(">. Triggering listeners ... \n");
		}

		this.FireTargets(TriggerStayTargets, offender.gameObject, FireTargetBase.TriggerType.STAY);

		if(Debug.isDebugBuild)
			Debug.Log(m_DebugMessage.ToString());
	}

	public void OnTriggerExit(Collider offender)
	{
		if(TriggerExitTargets.Count == 0)
			return;

		if(Debug.isDebugBuild) {
			m_DebugMessage.Length = 0;

			m_DebugMessage.Append("Interactable: [INFO] <");
			m_DebugMessage.Append(this.gameObject.transform.name);
			m_DebugMessage.Append("> had its trigger exited by <");
			m_DebugMessage.Append(offender.gameObject.transform.name);
			m_DebugMessage.Append(">. Triggering listeners ... \n");
		}

		this.FireTargets(TriggerExitTargets, offender.gameObject, FireTargetBase.TriggerType.EXIT);

		if(Debug.isDebugBuild)
			Debug.Log(m_DebugMessage.ToString());
	}

	protected void FireTargets(List<FireTargetBase> targets, GameObject offender, FireTargetBase.TriggerType type = FireTargetBase.TriggerType.NONE)
	{
		foreach(FireTargetBase target in targets) {
			if(!target) {
				if(Debug.isDebugBuild)
					m_DebugMessage.Append("Attempted to fire target, but target did not exist ...\n");

				continue;
			}

			if(Debug.isDebugBuild) {
				m_DebugMessage.Append("<");
				m_DebugMessage.Append(target.gameObject.transform.name);
				m_DebugMessage.Append("> firing.\n");
			}

			target.Fire(offender, type);
		}
	}
}
