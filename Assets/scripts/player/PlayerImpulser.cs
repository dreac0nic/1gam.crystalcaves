using System.Collections;
﻿using UnityEngine;

public class PlayerImpulser : Impulser
{
	public string AttackString = "Fire1";
	public string AltAttackString = "Fire2";
	public string ReloadString = "Fire3";

	public void Update()
	{
		if(Input.GetButton(AttackString)) {
			this.Impulse(ImpulseType.ATTACK);
		}

		if(Input.GetButton(AltAttackString)) {
			this.Impulse(ImpulseType.ALT_ATTACK);
		}

		if(Input.GetButtonDown(ReloadString)) {
			this.Impulse(ImpulseType.REFRESH);
		}
	}

	public override void Impulse(ImpulseType type)
	{
		switch(type) {
			default:
				base.Impulse(type);
				break;
		}
	}
}
