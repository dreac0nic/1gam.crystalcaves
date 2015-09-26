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
		[SerializeField] protected List<Transform> m_MountingAnchors;
		[SerializeField] protected List<Item> m_Items;
		protected Dictionary<Transform, Item> m_AnchorRelations;

		public InventorySlot()
		{
			m_Items = new List<Item>();
			m_MountingAnchors = new List<Transform>();
			m_AnchorRelations = new Dictionary<Transform, Item>();
		}

		public bool Pickup(Item new_item)
		{
			if(LimitItemCount && m_Items.Count >= MaximumItems) {
				return false;
			} else {
				if(!m_Items.Contains(new_item)) {
					m_Items.Add(new_item);
					AttachItemToAnchor(new_item);
				}
			}

			return true;
		}

		public bool Drop(Item drop_item)
		{
			if(m_Items.Contains(drop_item)) {
				m_Items.Remove(drop_item);
				DettachItemFromAnchor(drop_item);
				FillItemAnchors();
			}

			return true;
		}

		public void Resync(Inventory sync_inventory)
		{
			foreach(Item sync_item in m_Items) {
				sync_inventory.Pickup(sync_item);
			}
		}

		public void UpdateMounts()
		{
			foreach(KeyValuePair<Transform, Item> entry in m_AnchorRelations) {
				entry.Value.isVisible = true;
				entry.Value.transform.position = entry.Key.position;
				entry.Value.transform.rotation = entry.Key.rotation;
			}
		}

		protected void AttachItemToAnchor(Item attachment)
		{
			foreach(Transform anchor in m_MountingAnchors) {
				if(!m_AnchorRelations.ContainsKey(anchor)) {
					m_AnchorRelations[anchor] = attachment;
					attachment.isVisible = true;
					break;
				}
			}
		}

		protected void DettachItemFromAnchor(Item attachment)
		{
			foreach(KeyValuePair<Transform, Item> entry in m_AnchorRelations) {
				if(entry.Value == attachment) {
					m_AnchorRelations.Remove(entry.Key);
					attachment.isVisible = false;
					break;
				}
			}
		}

		protected void FillItemAnchors()
		{
			if(m_AnchorRelations.Count < m_MountingAnchors.Count && m_AnchorRelations.Count < m_Items.Count) {
				List<Item> unanchored_items = GetUnanchoredItems();

				foreach(Item home_item in unanchored_items) {
					AttachItemToAnchor(home_item);

					if(!(m_AnchorRelations.Count < m_MountingAnchors.Count && m_AnchorRelations.Count < m_Items.Count)) {
						break;
					}
				}
			}
		}

		protected List<Item> GetUnanchoredItems()
		{
			List<Item> unanchored_items = new List<Item>(m_Items);

			foreach(KeyValuePair<Transform, Item> entry in m_AnchorRelations) {
				unanchored_items.Remove(entry.Value);
			}

			return unanchored_items;
		}
	}

	public bool CanBeStolenFrom { get { return m_CanBeStolenFrom; } }

	[SerializeField] protected bool m_CanBeStolenFrom = true;
	public Item CurrentEquippedItem;
	public Transform DropLocation;
	public List<InventorySlot> Slots;

	public void Start()
	{
		foreach(InventorySlot start_slot in Slots) {
			start_slot.Resync(this);
		}
	}

	public void Update()
	{
		foreach(InventorySlot slot in Slots) {
			slot.UpdateMounts();
		}
	}

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
					drop_item.transform.position = DropLocation.position;
					drop_item.transform.localRotation = DropLocation.rotation;

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
