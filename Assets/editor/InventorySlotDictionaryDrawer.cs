using System.Collections;
using System.Collections.Generic;
﻿using UnityEngine;
﻿using UnityEditor;

[CustomPropertyDrawer(typeof(Inventory.SlotDictionary))]
public class InventorySlotDictionaryDrawer : PropertyDrawer
{
	protected const float BUTTON_WIDTH = 46.0f;
	protected bool m_Foldout = true;
	protected Dictionary<string, bool> m_SlotShowItems = new Dictionary<string, bool>();

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		Inventory.SlotDictionary slots = retrieveDictionary(property);
		Rect full_rect = position;
		position.height = 16.0f;

		Rect foldout_position = position;
		foldout_position.width -= (m_Foldout ? 2*(BUTTON_WIDTH) + 2.0f: 0.0f);
		m_Foldout = EditorGUI.Foldout(position, m_Foldout, label, false);
		//GUI.Button(foldout_position, "REF");

		if(m_Foldout) {
			Rect button_position = position;
			button_position.x += position.width - BUTTON_WIDTH;
			button_position.width = BUTTON_WIDTH;
			if(GUI.Button(button_position, new GUIContent("Add", "Add another slot to the inventory."), EditorStyles.miniButtonRight)) {
				string slot_name = "New Slot";
				Inventory.InventorySlot new_slot = new Inventory.InventorySlot();

				if(slots.ContainsKey(slot_name)) {
					for(int i = 1; i <= 100; ++i) {
						string new_name = slot_name + string.Format(" ({0:0})", i);

						if(!slots.ContainsKey(new_name)) {
							slot_name = new_name;
							break;
						}
					}
				}

				try {
					slots.Add(slot_name, new_slot);
				} catch(System.Exception except) {
					Debug.LogWarning(except.Message);
				}
			}

			button_position.x -= BUTTON_WIDTH;
			if(GUI.Button(button_position, new GUIContent("Clear", "Clear all inventory slots."), EditorStyles.miniButtonLeft)) {
				slots.Clear();
			}

			// Display all sub elements of the list!
			//*
			if(slots.Count > 0) {
				Rect slot_rect = position;
				slot_rect.width -= 20.0f;
				slot_rect.x += 0.5f*position.width - 0.5f*slot_rect.width;
				foreach(KeyValuePair<string, Inventory.InventorySlot> entry in slots) {
					string slot_name = entry.Key;
					Inventory.InventorySlot slot = entry.Value;
					slot_rect.y += 18.0f;
					
					EditorGUI.BeginChangeCheck();
					string new_slot_name = EditorGUI.TextField(slot_rect, GUIContent.none, slot_name);
					if(EditorGUI.EndChangeCheck()) {
						try {
							slots.Remove(slot_name);
							slots.Add(new_slot_name, slot);
							m_SlotShowItems[new_slot_name] = m_SlotShowItems[slot_name];
							m_SlotShowItems.Remove(slot_name);
						} catch(System.Exception except) {
							Debug.LogWarning(except.Message);
						}

						break;
					}

					slot_rect.y += 18.0f;
					Rect item_count_rect = slot_rect;
					if(slot.LimitItemCount) {
						item_count_rect.width = 120.0f;
					}
					slot.LimitItemCount = EditorGUI.ToggleLeft(item_count_rect, "Limit Item Count", slot.LimitItemCount);

					if(slot.LimitItemCount) {
						item_count_rect.x += item_count_rect.width + 6.0f;
						item_count_rect.width = slot_rect.width - item_count_rect.width - 6.0f;
						slot.MaximumItems = (uint)EditorGUI.Slider(item_count_rect, GUIContent.none, (int)slot.MaximumItems, 1.0f, 100.0f);
					}

					// Display the toggle between lists
					bool old_enabled = GUI.enabled;
					if(!m_SlotShowItems.ContainsKey(slot_name)) { // Do during button checks?
						m_SlotShowItems[slot_name] = true;
					}

					slot_rect.y += 18.0f;
					Rect list_toggle_controls = slot_rect;
					list_toggle_controls.width *= 0.25f;
					list_toggle_controls.x += 0.375f*slot_rect.width - 0.5f*list_toggle_controls.width;
					GUI.enabled = m_SlotShowItems[slot_name];
					if(GUI.Button(list_toggle_controls, "Mount Points", EditorStyles.miniButtonLeft)) {
						m_SlotShowItems[slot_name] = false;
					}

					GUI.enabled = !m_SlotShowItems[slot_name];
					list_toggle_controls.x += list_toggle_controls.width;
					if(GUI.Button(list_toggle_controls, "Items", EditorStyles.miniButtonRight)) {
						m_SlotShowItems[slot_name] = true;
					}
					GUI.enabled = old_enabled;

					// XXX: POLYMORPHISM PLZ
					if(m_SlotShowItems[slot_name]) {
						if(slot.Items.Count <= 0) {
							slot_rect.y += 18.0f;
							GUI.Box(slot_rect, "Add an item slot to attach a new item!", EditorStyles.helpBox);
						} else {
							foreach(Item item in slot.Items) {
								slot_rect.y += 18.0f;
								Rect list_item_rect = slot_rect;
								Rect list_item_del_rect = slot_rect;
								list_item_rect.width -= BUTTON_WIDTH - 2.0f;
								list_item_del_rect.width = BUTTON_WIDTH;
								list_item_del_rect.x += list_item_rect.width + 2.0f;
								EditorGUI.ObjectField(list_item_rect, GUIContent.none, item, typeof(Item), true);

								if(GUI.Button(list_item_del_rect, "Drop", EditorStyles.miniButton)) {
									slot.Items.Remove(item);
									Debug.LogWarning("ITEM REMOVED WITHOUT BEING DROPPED");
									break;
								}
							}
						}
					} else {
						if(slot.MountingAnchors.Count <= 0) {
							slot_rect.y += 18.0f;
							GUI.Box(slot_rect, "Add an anchor slot to attach a new mount!", EditorStyles.helpBox);
						} else {
							foreach(Transform anchor in slot.MountingAnchors) {
								slot_rect.y += 18.0f;
								Rect list_item_rect = slot_rect;
								Rect list_item_del_rect = slot_rect;
								list_item_rect.width -= BUTTON_WIDTH - 2.0f;
								list_item_del_rect.width = BUTTON_WIDTH;
								list_item_del_rect.x += list_item_rect.width + 2.0f;
								EditorGUI.ObjectField(list_item_rect, GUIContent.none, anchor, typeof(Transform), true);

								if(GUI.Button(list_item_del_rect, "Delete", EditorStyles.miniButton)) {
									slot.MountingAnchors.Remove(anchor);
									break;
								}
							}
						}
					}

					slot_rect.y += 18.0f;
					Rect add_new_item_rect = slot_rect;
					add_new_item_rect.width *= 0.35f;
					add_new_item_rect.x += 0.5f*slot_rect.width - 0.5f*add_new_item_rect.width;
					if(GUI.Button(add_new_item_rect, "Add", EditorStyles.miniButton)) {
						if(m_SlotShowItems[slot_name]) {
							slot.Items.Add(null);
						} else {
							slot.MountingAnchors.Add(null);
						}
					}
				}
			} else {
				position.y += 18.0f;
				position.width -= 32.0f;
				position.x += 0.5f*full_rect.width - 0.5f*position.width;
				GUI.Box(position, "Add an inventory slot to get started!", EditorStyles.helpBox);
			}
			//*/
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float proposed_height = 16.0f;

		if(m_Foldout) {
			if(retrieveDictionary(property).Count > 0.0f) {
				foreach(KeyValuePair<string, Inventory.InventorySlot> entry in retrieveDictionary(property)) {
					proposed_height += getSlotHeight(entry.Key, entry.Value);
				}
			} else {
				proposed_height += 18.0f;
			}
		}

		return proposed_height;
	}

	protected float getSlotHeight(string slot_name, Inventory.InventorySlot slot)
	{
		bool show_items = (m_SlotShowItems.ContainsKey(slot_name) && m_SlotShowItems[slot_name]);
		int selected_list_count = (show_items ? slot.Items.Count : slot.MountingAnchors.Count);
		float proposed_height = 3*18.0f;
		proposed_height += 18.0f*(selected_list_count > 0 ? selected_list_count : 1);
		proposed_height += 18.0f; // Add button!

		return proposed_height;
	}

	protected Inventory.SlotDictionary retrieveDictionary(SerializedProperty property)
	{
		Object target = property.serializedObject.targetObject;
		Inventory.SlotDictionary slots = (Inventory.SlotDictionary)fieldInfo.GetValue(target);

		if(slots == null) {
			slots = new Inventory.SlotDictionary();
			fieldInfo.SetValue(target, slots);
		}

		return slots;
	}
}
