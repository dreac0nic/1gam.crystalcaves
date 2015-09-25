using System.Collections;
﻿using UnityEngine;
﻿using UnityEditor;


public class InventorySlotDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		SerializedProperty p_LimitItemCount = property.FindPropertyRelative("LimitItemCount");
		SerializedProperty p_MountingAnchors = property.FindPropertyRelative("MountingAnchors");
		SerializedProperty p_Items = property.FindPropertyRelative("m_Items");

		label = EditorGUI.BeginProperty(position, label, property);
		EditorGUI.LabelField(position, label);
		position.height = 16.0f;
		EditorGUI.indentLevel++;
		position = EditorGUI.IndentedRect(position);

		EditorGUI.indentLevel = 0;
		position.y += 18.0f;
		EditorGUI.PropertyField(position, p_LimitItemCount);
		if(p_LimitItemCount.boolValue) {
			position.y += 18.0f;
			EditorGUI.PropertyField(position, property.FindPropertyRelative("MaximumItems"));
		}

		position.y += 18.0f;
		EditorGUI.PropertyField(position, p_MountingAnchors);
		if(EditorGUI.GetPropertyHeight(p_MountingAnchors) > 16.0f) {
			SerializedProperty size_property = p_MountingAnchors.Copy();
			EditorGUI.indentLevel++;

			size_property.Next(true);
			size_property.Next(true);
			position.y += 18.0f;
			EditorGUI.PropertyField(position, size_property);

			foreach(SerializedProperty item in p_MountingAnchors) {
				position.y += 18.0f;
				EditorGUI.PropertyField(position, item);
			}
			EditorGUI.indentLevel--;
		}

		position.y += 18.0f;
		EditorGUI.PropertyField(position, p_Items);
		if(EditorGUI.GetPropertyHeight(p_Items) > 16.0f) {
			SerializedProperty size_property = p_Items.Copy();
			EditorGUI.indentLevel++;

			size_property.Next(true);
			size_property.Next(true);
			position.y += 18.0f;
			EditorGUI.PropertyField(position, size_property);

			foreach(SerializedProperty item in p_Items) {
				position.y += 18.0f;
				EditorGUI.PropertyField(position, item);
			}
			EditorGUI.indentLevel--;
		}

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float mounting_anchor_height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("MountingAnchors"));
		float items_anchor_height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("m_Items"));

		return 16.0f + 18.0f + (property.FindPropertyRelative("LimitItemCount").boolValue ? 18.0f : 0.0f) + mounting_anchor_height + items_anchor_height;
	}
}
