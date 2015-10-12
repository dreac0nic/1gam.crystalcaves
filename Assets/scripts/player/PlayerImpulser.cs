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
		Shootable weapon = (held_item ? held_item.GetComponent<Shootable>() : null);
		Transform camera_anchor = (movement_controller ? movement_controller.CameraAnchor : null);

		if(held_item && camera_anchor) {
			switch(type) {
				case ImpulseType.ATTACK:
					if(weapon) {
						weapon.Fire(camera_anchor);
					}

					break;

				case ImpulseType.REFRESH:
					if(weapon) {
						weapon.Reload();
					}

					break;

				default:
					base.Impulse(type);
					break;
			}
		}
	}
}
