using System.Collections;
using System.Collections.Generic;
﻿using UnityEngine;
﻿using UnityEditor;

[CustomPropertyDrawer(typeof(Inventory.InventorySlot))]
public class InventorySlotDrawer : PropertyDrawer
{
	private class SlotEditorState
	{
		public bool ShowAnchors = false;
	}

	private class StateDictionary : Dictionary<string, SlotEditorState> {}

	private const float SLOT_PADDING = 18.0f;
	private const float CONTAINER_PADDING = 4.0f;
	private const float LIST_PADDING = 18.0f;
	private const float TOGGLE_BUTTON_WIDTH = 64.0f;

	private static StateDictionary m_PropertyStates = new StateDictionary();

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		bool old_gui_state;
		Rect carrot = position;
		SerializedProperty p_Name = property.FindPropertyRelative("Name");
		SerializedProperty p_FocusedList = property.FindPropertyRelative("m_MountingAnchors");
		SerializedProperty p_LimitItemCount = property.FindPropertyRelative("LimitItemCount");

		// Setup standard states
		carrot.height = 16.0f;
		if(!m_PropertyStates.ContainsKey(property.propertyPath)) {
			m_PropertyStates[property.propertyPath] = new SlotEditorState();
		}

		// Draw header label
		label = EditorGUI.BeginProperty(position, label, property);
		EditorGUI.LabelField(carrot, label);
		EditorGUI.indentLevel++;

		carrot.y += 18.0f;
		EditorGUI.PropertyField(carrot, p_Name, GUIContent.none);

		carrot.y += 18.0f;
		Rect item_count_rect = carrot;
		item_count_rect.width = (p_LimitItemCount.boolValue ? 0.5f*item_count_rect.width : item_count_rect.width);
		p_LimitItemCount.boolValue = EditorGUI.ToggleLeft(item_count_rect, "Limit Items", p_LimitItemCount.boolValue);
		if(p_LimitItemCount.boolValue) {
			item_count_rect.x += item_count_rect.width;
			EditorGUI.PropertyField(item_count_rect, property.FindPropertyRelative("MaximumItems"), GUIContent.none);
		}

		// Draw controls for toggling between lists
		old_gui_state = GUI.enabled;

		carrot.y += 18.0f;
		Rect toggle_buttons_rect = carrot;
		toggle_buttons_rect.x += 0.5f*carrot.width - TOGGLE_BUTTON_WIDTH;
		toggle_buttons_rect.width = TOGGLE_BUTTON_WIDTH;
		GUI.enabled = !m_PropertyStates[property.propertyPath].ShowAnchors;
		if(GUI.Button(toggle_buttons_rect, "Mounts", EditorStyles.miniButtonLeft)) {
			m_PropertyStates[property.propertyPath].ShowAnchors = true;
		}

		toggle_buttons_rect.x += TOGGLE_BUTTON_WIDTH;
		GUI.enabled = m_PropertyStates[property.propertyPath].ShowAnchors;
		if(GUI.Button(toggle_buttons_rect, "Items", EditorStyles.miniButtonRight)) {
			m_PropertyStates[property.propertyPath].ShowAnchors = false;
		}

		GUI.enabled = old_gui_state;

		// Draw focused list
		if(!m_PropertyStates[property.propertyPath].ShowAnchors) {
			p_FocusedList = property.FindPropertyRelative("m_Items");
		}

		carrot.y += 18.0f;
		Rect list_rect = carrot;
		GUIContent list_label = label;
		list_label.text = ObjectNames.NicifyVariableName(p_FocusedList.name);
		list_rect.x += LIST_PADDING;
		list_rect.width -= 2*LIST_PADDING;
		list_rect.height = EditorGUI.GetPropertyHeight(p_FocusedList);
		EditorGUI.PropertyField(list_rect, p_FocusedList, list_label, true);

		EditorGUI.indentLevel--;
		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float current_list_height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative((m_PropertyStates.ContainsKey(property.propertyPath) && m_PropertyStates[property.propertyPath].ShowAnchors) ? "m_MountingAnchors" : "m_Items"));

		return 16.0f + 3*18.0f + current_list_height;
	}
}
