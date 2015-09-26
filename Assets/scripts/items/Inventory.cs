using System.Collections;
using System.Collections.Generic;
﻿using UnityEngine;

public class Inventory : MonoBehaviour
{
	[System.Serializable]
	public class InventorySlot
	{
		public string Name = "New Slot";
		public bool LimitItemCount = false;
		public uint MaximumItems = 2;
		public List<Transform> MountingAnchors;
		public List<Item> Items;

		public InventorySlot()
		{
			Items = new List<Item>();
			MountingAnchors = new List<Transform>();
		}

		public Transform GetFreeMountingAnchor()
		{
			return null;
		}
	}

	public bool CanBeStolenFrom { get { return m_CanBeStolenFrom; } }

	[SerializeField] protected bool m_CanBeStolenFrom = true;
	public Item CurrentEquippedItem;
	public List<InventorySlot> Slots;

	public bool Pickup(Item new_item)
	{
		InventorySlot slot;
		if(this.retrieveSlot(new_item.Slot, out slot) && !slot.Items.Contains(new_item)) {
			slot.Items.Add(new_item);

			return true;
		}

		return false;
	}

	public bool Drop(Item drop_item)
	{
		InventorySlot slot;
		if(this.retrieveSlot(drop_item.Slot, out slot) && slot.Items.Contains(drop_item)) {
			slot.Items.Remove(drop_item);

			return true;
		}

		return false;
	}

	private bool hasSlot(string name)
	{
		foreach(InventorySlot slot in Slots) {
			if(slot.Name == name) {
				return true;
			}
		}

		return false;
	}

	private bool retrieveSlot(string name)
	{
		return this.hasSlot(name);
	}

	private bool retrieveSlot(string name, out InventorySlot ret_slot)
	{
		ret_slot = null;

		foreach(InventorySlot slot in Slots) {
			if(slot.Name == name) {
				ret_slot = slot;

				return true;
			}
		}

		return false;
	}
}
