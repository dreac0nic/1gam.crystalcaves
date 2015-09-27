using System.Collections;
using System.Collections.Generic;
﻿using UnityEngine;

public class Inventory : MonoBehaviour
{
	[System.Serializable]
	public class InventorySlot
	{
		public string Name;
		public bool LimitItemCount;
		public uint MaximumItems;
		public List<Transform> MountingAnchors;
		public List<Item> Items;
		public Dictionary<Transform, Item> AnchorRelations;

		public InventorySlot()
		{
			Name = "New Slot";
			LimitItemCount = false;
			MaximumItems = 2;
			Items = new List<Item>();
			MountingAnchors = new List<Transform>();
			AnchorRelations = new Dictionary<Transform, Item>();
		}

		public Transform GetFreeAnchor()
		{
			foreach(Transform anchor in MountingAnchors) {
				if(!AnchorRelations.ContainsKey(anchor)) {
					return anchor;
				}
			}

			return null;
		}

		public Transform GetAnchorFromItem(Item anchored_item)
		{
			foreach(KeyValuePair<Transform, Item> relation in AnchorRelations) {
				if(relation.Value == anchored_item) {
					return relation.Key;
				}
			}

			return null;
		}

		public List<Item> GetUnanchoredItems()
		{
			List<Item> unanchored_items = new List<Item>(Items);

			foreach(Item anchored_item in AnchorRelations.Values) {
				unanchored_items.Remove(anchored_item);
			}

			return unanchored_items;
		}
	}

	public bool CanBeStolenFrom { get { return m_CanBeStolenFrom; } }

	[SerializeField] protected bool m_CanBeStolenFrom = true;
	public Item CurrentEquippedItem;
	public Transform DropLocation;
	[SerializeField] protected List<InventorySlot> Slots;

	public void Awake()
	{
		if(!DropLocation) {
			DropLocation = this.transform;
		}
	}

	public void Start()
	{
		foreach(InventorySlot slot in Slots) {
			foreach(Item item in slot.Items) {
				Pickup(item);
			}
		}
	}

	public void Update()
	{
		foreach(InventorySlot slot in Slots) {
			foreach(KeyValuePair<Transform, Item> relation in slot.AnchorRelations) {
				relation.Value.transform.position = Vector3.Lerp(relation.Value.transform.position, relation.Key.position, 25.0f*Time.time);
				relation.Value.transform.rotation = Quaternion.Slerp(relation.Value.transform.rotation, relation.Key.rotation, 35.0f*Time.time);
			}
		}
	}

	public bool Pickup(Item new_item)
	{
		InventorySlot slot;
		if(this.retrieveSlot(new_item.Slot, out slot)) {
			if(!new_item.hasOwner || new_item.Owner.Drop(new_item, true)) {
				if(!slot.LimitItemCount || slot.Items.Count < slot.MaximumItems) {
					if(!slot.Items.Contains(new_item)) {
						slot.Items.Add(new_item);
					}

					new_item.Owner = this;
					new_item.transform.SetParent(this.transform, false);
					new_item.transform.localPosition = Vector3.zero;
					new_item.transform.localRotation = Quaternion.identity;

					Transform anchor = slot.GetFreeAnchor();
					if(anchor) {
						slot.AnchorRelations[anchor] = new_item;
						new_item.isVisible = true;
					}
				}

				return true;
			}
		}

		return false;
	}

	public bool Drop(Item drop_item, bool being_stolen = false)
	{
		InventorySlot slot;
		if(this.retrieveSlot(drop_item.Slot, out slot)) {
			if(!being_stolen || (m_CanBeStolenFrom && drop_item.CanBeStolen)) {
				if(slot.Items.Contains(drop_item)) {
					slot.Items.Remove(drop_item);

					Transform anchor = slot.GetFreeAnchor();
					if(anchor) {
						List<Item> available_items = slot.GetUnanchoredItems();

						if(available_items.Count > 0) {
							slot.AnchorRelations[anchor] = available_items[0];
						} else {
							slot.AnchorRelations.Remove(anchor);
						}
					}

					drop_item.transform.SetParent(null, true);
					drop_item.Owner = null;

					if(DropLocation) {
						drop_item.transform.position = DropLocation.position;
						drop_item.transform.localRotation = DropLocation.rotation;
					}

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
