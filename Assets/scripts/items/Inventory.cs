using System.Collections;
using System.Collections.Generic;
﻿using UnityEngine;

public class Inventory : MonoBehaviour
{
	[System.Serializable]
	public class InventorySlot
	{
		public List<Item> Items { get { return m_Items; } }

		public string Name = "general";
		public uint MaximumItems = 2;
		public List<Transform> MountingAnchors;
		protected List<Item> m_Items;

		public InventorySlot()
		{
			m_Items = new List<Item>();
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
	public List<InventorySlot> Slots;

	public void Pickup(Item one_true_desire)
	{
		if(one_true_desire && !this.HasItem(one_true_desire) && !one_true_desire.IsOwned) {
			InventorySlot slot = findInventorySlot(one_true_desire.Slot);
			slot.Items.Add(one_true_desire);
			one_true_desire.Owner = this;

			Transform possible_anchor = slot.GetFreeMountingAnchor();

			if(possible_anchor) {
				one_true_desire.transform.SetParent(possible_anchor, false);
				one_true_desire.transform.localPosition = Vector3.zero;
				one_true_desire.transform.localRotation = Quaternion.identity;
				Rigidbody body = one_true_desire.GetComponent<Rigidbody>();
				body.isKinematic = true;

				foreach(Collider coll in one_true_desire.GetComponentsInChildren<Collider>()) {
					coll.enabled = false;
				}
			}
		}
	}

	public void Drop(Item screw_you_sucky_item)
	{
		if(screw_you_sucky_item && this.HasItem(screw_you_sucky_item)) {
			findInventorySlot(screw_you_sucky_item.Slot).Items.Remove(screw_you_sucky_item);
			screw_you_sucky_item.Owner = null;
		}
	}

	public bool HasItem(Item possession)
	{
		return possession && findInventorySlot(possession.Slot).Items.Contains(possession);
	}

	protected InventorySlot findInventorySlot(string name)
	{
		// XXX: This should be alleviated with a dictionary-style system.
		foreach(InventorySlot slot in Slots) {
			if(slot.Name == name) {
				return slot;
			}
		}

		Debug.Log("Could not find slot: " + name);

		return null;
	}
}
