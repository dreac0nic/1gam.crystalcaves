using UnityEngine;
using System.Collections;

public class FireTargetBase : MonoBehaviour
{
	public enum TriggerType {NONE, ENTER, STAY, EXIT};

	public virtual void Fire(GameObject offender, TriggerType type = TriggerType.NONE)
	{
		if(Debug.isDebugBuild)
			if(type == TriggerType.NONE)
				Debug.Log("FIRE!");
			else
				Debug.Log("FIRE TRIGGER [" + type + "] FROM <" + offender.transform.name + ">");
	}
}
