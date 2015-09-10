using UnityEngine;
using System.Collections;

public class FireTargetBase : MonoBehaviour
{
	public enum TriggerType {GENERAL, ENTER, STAY, EXIT};

	public virtual void Fire(GameObject offender, TriggerType type = TriggerType.GENERAL)
	{
		if(Debug.isDebugBuild)
			Debug.Log("FIRE TRIGGER [" + type + "] FROM <" + offender.transform.name + ">");
	}
}
