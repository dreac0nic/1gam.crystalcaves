using System.Collections;
using System.Collections.Generic;
﻿using UnityEngine;

public class Inventory : MonoBehaviour
{
	[System.Serializable] public class SlotDictionary : SerializableDictionary<string, InventorySlot> {}

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
			foreach(Transform anchor in MountingAnchors) {
				if(anchor.GetComponentsInChildren<Item>().Length <= 0) {
					return anchor;
				}
			}

			return null;
		}
	}

	public bool CanBeStolenFrom { get { return m_CanBeStolenFrom; } }

	[SerializeField] protected bool m_CanBeStolenFrom = true;
	public Item CurrentEquippedItem;
	public InventorySlot TestSlot;
	public SlotDictionary Slots;
	public List<InventorySlot> TestSlots;

	public bool Pickup(Item new_item)
	{
		if(Slots.ContainsKey(new_item.Slot) && !Slots[new_item.Slot].Items.Contains(new_item)) {
			Slots[new_item.Slot].Items.Add(new_item);

			return true;
		}

		return false;
	}

	public bool Drop(Item drop_item)
	{
		if(Slots.ContainsKey(drop_item.Slot) && Slots[drop_item.Slot].Items.Contains(drop_item)) {
			Slots[drop_item.Slot].Items.Remove(drop_item);

			return true;
		}

		return false;
	}

	public void Update()
	{
	}
}
