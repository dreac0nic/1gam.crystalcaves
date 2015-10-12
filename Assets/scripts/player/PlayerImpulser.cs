using System.Collections;
﻿using UnityEngine;

public class PlayerImpulser : Impulser
{
	public string AttackInput = "Fire1";
	public string AltAttackInput = "Fire2";
	public string ReloadInput = "Fire3";
	public string InteractInput = "Submit";
	public string CycleEquipmentInput = "Mouse ScrollWheel";
	public string LastUsedEquipmentInput = "Jump";
	public string DropCurrentEquipmentInput = "Cancel";

	public void Update()
	{
		if(Input.GetButton(AttackInput)) {
			this.Impulse(ImpulseType.ATTACK);
		}

		if(Input.GetButton(AltAttackInput)) {
			this.Impulse(ImpulseType.ALT_ATTACK);
		}

		if(Input.GetButtonDown(ReloadInput)) {
			this.Impulse(ImpulseType.REFRESH);
		}

		if(Input.GetButtonDown(InteractInput)) {
			this.Impulse(ImpulseType.INTERACT);
		}

		if(Input.GetButtonDown(LastUsedEquipmentInput)) {
			this.Impulse(ImpulseType.SWAP_LAST_USED_ITEM);
		}

		if(Mathf.Abs(Input.GetAxis(CycleEquipmentInput)) > float.Epsilon) {
			this.Impulse(Input.GetAxis(CycleEquipmentInput) < 0.0f ? ImpulseType.CYCLE_NEXT_ITEM : ImpulseType.CYCLE_PREV_ITEM);
		}

		if(Input.GetButtonDown(DropCurrentEquipmentInput)) {
			this.Impulse(ImpulseType.DROP_ITEM);
		}
	}

	public override void Impulse(ImpulseType type)
	{
		FirstPersonMovementController movement_controller = GetComponent<FirstPersonMovementController>();
		Inventory player_inventory = GetComponent<Inventory>();
		Item held_item = (player_inventory ? player_inventory.CurrentEquippedItem : null);
		Impulser item_impulser = (held_item ? held_item.GetComponent<Impulser>() : null);
		FirstPersonUser fps_user = GetComponent<FirstPersonUser>();

		switch(type) {
			case ImpulseType.INTERACT:
				if(fps_user) {
					fps_user.Interact();
				}
				break;

			case ImpulseType.SWAP_LAST_USED_ITEM:
				if(player_inventory) {
					player_inventory.SwapToLastEquipment();
				}
				break;

			case ImpulseType.CYCLE_NEXT_ITEM:
				if(player_inventory) {
					player_inventory.CycleEquipment();
				}
				break;

			case ImpulseType.CYCLE_PREV_ITEM:
				if(player_inventory) {
					player_inventory.CycleEquipment(-1);
				}
				break;

			case ImpulseType.DROP_ITEM:
				if(player_inventory) {
					player_inventory.DropCurrentEquipment();
				}
				break;

			case ImpulseType.ATTACK:
				if(item_impulser) {
					item_impulser.Impulse(type); // XXX: Lol. This is embarrasing.
				}
				break;

			case ImpulseType.ALT_ATTACK:
				if(item_impulser) {
					item_impulser.Impulse(type); // XXX: Lol. This is embarrasing.
				}
				break;

			case ImpulseType.REFRESH:
				if(item_impulser) {
					item_impulser.Impulse(type); // XXX: Lol. This is embarrasing.
				}
				break;

			default:
				base.Impulse(type);
				break;
		}
	}
}
