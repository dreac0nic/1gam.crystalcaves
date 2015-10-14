using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof (Collider))]
public class Interactable : MonoBehaviour
{
	public List<FireTargetBase> InteractionTargets;

	private StringBuilder m_DebugMessage;

	public void Awake()
	{
		m_DebugMessage = new StringBuilder();
	}

	protected void processInteraction(GameObject offender, FireTargetBase.TriggerType type = FireTargetBase.TriggerType.GENERAL)
	{
		if(InteractionTargets.Count == 0)
			return;

		if(Debug.isDebugBuild) {
			m_DebugMessage.Append("Interactable: [INFO] <");
			m_DebugMessage.Append(this.gameObject.name);
			m_DebugMessage.Append("> was interacted with. Triggering listeners ... \n");
		}

		this.fireTargets(offender, type);

		if(Debug.isDebugBuild) {
			Debug.Log(m_DebugMessage.ToString());

			m_DebugMessage.Length = 0; // .NET 4.0 anytime, Unity :)
		}
	}

	private void fireTargets(GameObject offender, FireTargetBase.TriggerType type = FireTargetBase.TriggerType.GENERAL)
	{
		foreach(FireTargetBase target in InteractionTargets) {
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
