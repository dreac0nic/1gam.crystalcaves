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
		FirstPersonMovementController movement_controller = GetComponent<FirstPersonMovementController>();
		Inventory player_inventory = GetComponent<Inventory>();
		Item held_item = (player_inventory ? player_inventory.CurrentEquippedItem : null);
		Impulser item_impulser = (held_item ? held_item.GetComponent<Impulser>() : null);

		if(held_item && item_impulser) {
			switch(type) {
				case ImpulseType.ATTACK:
					item_impulser.Impulse(type);
					break;

				case ImpulseType.REFRESH:
					item_impulser.Impulse(type);
					break;

				default:
					base.Impulse(type);
					break;
			}
		}
	}
}
