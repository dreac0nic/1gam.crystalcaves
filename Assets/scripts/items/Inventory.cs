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
		[SerializeField] protected List<Transform> MountingAnchors;
		[SerializeField] protected List<Item> Items;

		public InventorySlot()
		{
			Items = new List<Item>();
			MountingAnchors = new List<Transform>();
		}

		public bool Pickup(Item new_item)
		{
			if(!Items.Contains(new_item) && (!LimitItemCount || Items.Count < MaximumItems)) {
				Items.Add(new_item);

				return true;
			}

			return false;
		}

		public bool Drop(Item drop_item)
		{
			if(Items.Contains(drop_item)) {
				Items.Remove(drop_item);

				return true;
			}

			return false;
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
		if(this.retrieveSlot(new_item.Slot, out slot)) { // XXX: Collapse, maybe...
			if(!new_item.hasOwner || new_item.Owner.Drop(new_item)) {
				if(slot.Pickup(new_item)) {
					new_item.Owner = this;
					new_item.transform.SetParent(this.transform, false);
					new_item.transform.localPosition = Vector3.zero;
					new_item.transform.localRotation = Quaternion.identity;

					return true;
				}
			}
		}

		return false;
	}

	public bool Drop(Item drop_item, bool being_stolen = false)
	{
		InventorySlot slot;
		if(this.retrieveSlot(drop_item.Slot, out slot)) {
			if(!being_stolen || (m_CanBeStolenFrom && drop_item.CanBeStolen)) {
				if(slot.Drop(drop_item)) {
					drop_item.Owner = null;
					drop_item.transform.SetParent(null);

					return true;
				}
			}
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
