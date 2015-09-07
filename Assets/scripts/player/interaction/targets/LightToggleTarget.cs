using UnityEngine;
using System.Collections;

[RequireComponent(typeof (Light))]
public class LightToggleTarget : FireTargetBase
{
	protected Light m_TargetLight;

	public void Start() {
		m_TargetLight = GetComponent<Light>();
	}

	public override void Fire(GameObject offender, TriggerType type = TriggerType.NONE)
	{
		m_TargetLight.enabled = !m_TargetLight.enabled;
	}
}
