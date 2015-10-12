using System.Collections;
﻿using UnityEngine;

public class PlayerImpulser : Impulser
{
	public string AttackInput = "Fire1";
	public string AltAttackInput = "Fire2";
	public string ReloadInput = "Fire3";
	public string InteractInput = "Submit";
	public string CycleEquipmentAxis = "Mouse ScrollWheel";
	public string CycleEquipmentNextInput = "";
	public string CycleEquipmentPrevInput = "";
	public string LastUsedEquipmentInput = "Jump";
	public string DropCurrentEquipmentInput = "Cancel";

	public void Update()
	{
		if(!string.IsNullOrEmpty(AttackInput) && Input.GetButton(AttackInput)) {
			this.Impulse(ImpulseType.ATTACK);
		}

		if(!string.IsNullOrEmpty(AltAttackInput) && Input.GetButton(AltAttackInput)) {
			this.Impulse(ImpulseType.ALT_ATTACK);
		}

		if(!string.IsNullOrEmpty(ReloadInput) && Input.GetButtonDown(ReloadInput)) {
			this.Impulse(ImpulseType.REFRESH);
		}

		if(!string.IsNullOrEmpty(InteractInput) && Input.GetButtonDown(InteractInput)) {
			this.Impulse(ImpulseType.INTERACT);
		}

		if(!string.IsNullOrEmpty(LastUsedEquipmentInput) && Input.GetButtonDown(LastUsedEquipmentInput)) {
			this.Impulse(ImpulseType.SWAP_LAST_USED_ITEM);
		}

		if(!string.IsNullOrEmpty(CycleEquipmentAxis) && Mathf.Abs(Input.GetAxis(CycleEquipmentAxis)) > float.Epsilon) {
			this.Impulse(Input.GetAxis(CycleEquipmentAxis) < 0.0f ? ImpulseType.CYCLE_NEXT_ITEM : ImpulseType.CYCLE_PREV_ITEM);
		} else if(!string.IsNullOrEmpty(CycleEquipmentNextInput) && Input.GetButtonDown(CycleEquipmentNextInput)) {
			this.Impulse(ImpulseType.CYCLE_NEXT_ITEM);
		} else if(!string.IsNullOrEmpty(CycleEquipmentPrevInput) && Input.GetButtonDown(CycleEquipmentPrevInput)) {
			this.Impulse(ImpulseType.CYCLE_PREV_ITEM);
		}

		if(!string.IsNullOrEmpty(DropCurrentEquipmentInput) && Input.GetButtonDown(DropCurrentEquipmentInput)) {
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
