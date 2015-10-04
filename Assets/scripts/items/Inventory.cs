using System.Text;
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
	public Transform ViewmodelAnchor;
	public Transform DropLocation;
	[SerializeField] protected List<InventorySlot> Slots;

	[Header("REMOVE THESE WHEN IMPULSER EXISTS")]
	public string ItemCycleInput = "Mouse ScrollWheel";
	public string LastItemInput = "Fire3";
	public string DropItemInput = "Jump";

	protected GameObject m_CurrentViewmodel;
	protected Item m_LastEquippedItem;

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

		if(Input.GetButtonDown(LastItemInput)) {
			Equip(m_LastEquippedItem);
		} else if(Input.GetButtonDown(DropItemInput) && CurrentEquippedItem) {
			Drop(CurrentEquippedItem);

			if(!Equip(m_LastEquippedItem) && !CycleEquipment(-1)) {
				CurrentEquippedItem = null;

				if(m_CurrentViewmodel) {
					Destroy(m_CurrentViewmodel);
				}
			}
		} else if(Mathf.Abs(Input.GetAxis(ItemCycleInput)) > float.Epsilon) {
			CycleEquipment(Input.GetAxis(ItemCycleInput) < 0.0f ? 1 : -1);
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

					if(!CurrentEquippedItem) {
						Equip(new_item);
					} else {
						Transform anchor = slot.GetFreeAnchor();
						if(anchor) {
							slot.AnchorRelations[anchor] = new_item;
							new_item.isVisible = true;
						}
					}

					// Add as "previous item" if none exists, and this is not already the current item
					if(!m_LastEquippedItem && new_item != CurrentEquippedItem) {
						m_LastEquippedItem = new_item;
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

					Transform anchor = slot.GetAnchorFromItem(drop_item);
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

	protected bool Equip(Item focus_item)
	{
		Transform anchor;
		InventorySlot slot;

		if(focus_item && this.retrieveSlot(focus_item.Slot, out slot) && slot.Items.Contains(focus_item)) {
			// Destroy/Instantiate viewmodels
			if(m_CurrentViewmodel) {
				Destroy(m_CurrentViewmodel);
			}

			if(focus_item.ViewmodelPrefab && ViewmodelAnchor) {
				m_CurrentViewmodel = (GameObject)Instantiate(focus_item.ViewmodelPrefab);
				m_CurrentViewmodel.transform.SetParent(ViewmodelAnchor, false);
			} else {
				m_CurrentViewmodel = null;
			}

			// Remove the new focused item from a slot if it was slotted.
			anchor = slot.GetAnchorFromItem(focus_item);
			if(CurrentEquippedItem && CurrentEquippedItem.Slot == focus_item.Slot && slot.Items.Contains(CurrentEquippedItem) && anchor) {
				slot.AnchorRelations[anchor] = CurrentEquippedItem;
			} else if(anchor) {
				List<Item> available_items = slot.GetUnanchoredItems();

				if(available_items.Count > 0) {
					slot.AnchorRelations[anchor] = available_items[0];
				} else {
					slot.AnchorRelations.Remove(anchor);
				}
			}

			// Store the old equipped item in a slot if one is available, and it is not already slotted
			InventorySlot old_item_slot;
			if(CurrentEquippedItem && retrieveSlot(CurrentEquippedItem.Slot, out old_item_slot) && old_item_slot.Items.Contains(CurrentEquippedItem) && !old_item_slot.GetAnchorFromItem(CurrentEquippedItem)) {
				anchor = old_item_slot.GetFreeAnchor();

				if(anchor) {
					old_item_slot.AnchorRelations[anchor] = CurrentEquippedItem;
				}
			}

			// Update references
			if(old_item_slot != null && old_item_slot.Items.Contains(CurrentEquippedItem)) {
				m_LastEquippedItem = CurrentEquippedItem;
			} else {
				m_LastEquippedItem = null;
			}

			CurrentEquippedItem = focus_item;
		} else {
			return false;
		}

		return true;
	}

	protected bool CycleEquipment(int offset = 1)
	{
		bool return_value = false;
		int equipped_index = -1;
		List<Item> held_items;

		if(offset != 0) {
			held_items = getAllItems();

			if(held_items.Count > 1 || (held_items.Count == 1 && held_items[0] != CurrentEquippedItem)) {
				for(equipped_index = 0; equipped_index < held_items.Count; ++equipped_index) {
					if(held_items[equipped_index] == CurrentEquippedItem) {
						break;
					}
				}

				int cycle_item = equipped_index + offset;
				if(cycle_item < 0) {
					cycle_item = held_items.Count - (Mathf.Abs(cycle_item)%held_items.Count);
				} else if(cycle_item >= held_items.Count) {
					cycle_item = cycle_item%held_items.Count;
				}

				return_value = Equip(held_items[cycle_item]);
			}
		}

		return return_value;
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

	private List<Item> getAllItems()
	{
		List<Item> items = new List<Item>();

		foreach(InventorySlot slot in Slots) {
			foreach(Item held_item in slot.Items) {
				items.Add(held_item);
			}
		}

		return items;
	}
}
