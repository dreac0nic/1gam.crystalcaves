using System.Collections;
﻿using UnityEngine;

public class Impulser : MonoBehaviour
{
	public enum ImpulseType {
		INTERACT,
		ATTACK, ALT_ATTACK, REFRESH,
		DROP_ITEM, CYCLE_NEXT_ITEM, CYCLE_PREV_ITEM, SWAP_LAST_USED_ITEM
	};

	public virtual void Impulse(ImpulseType type)
	{
		if(Debug.isDebugBuild) {
			Debug.Log("Impulse received on " + this.gameObject.name + ": " + type.ToString());
		}
	}
}
