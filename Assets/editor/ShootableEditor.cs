using System.Collections;
﻿using UnityEngine;
﻿using UnityEditor;

[CustomEditor(typeof(Shootable))]
public class ShootableEditor : Editor
{
	public override void OnInspectorGUI()
	{
		bool old_gui_enabled = GUI.enabled;

		// Retrieve Common-use Properties
		serializedObject.Update();
		SerializedProperty p_RaycastEnabled = serializedObject.FindProperty("RaycastProjectileEnabled");

		EditorGUILayout.PropertyField(serializedObject.FindProperty("Barrel"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("CycleTime"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("ProjectilesPerShot"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("SpreadAngle"));

		// Toggle buttons for raycast/projectile.
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Projectile", EditorStyles.boldLabel);
		GUILayout.BeginHorizontal();
		old_gui_enabled = GUI.enabled;
		GUI.enabled = (old_gui_enabled && !p_RaycastEnabled.boolValue);
		if(GUILayout.Button("Raycast", EditorStyles.miniButtonLeft)) {
			p_RaycastEnabled.boolValue = true;
		}

		GUI.enabled = (old_gui_enabled && p_RaycastEnabled.boolValue);
		if(GUILayout.Button("Projectile", EditorStyles.miniButtonRight)) {
			p_RaycastEnabled.boolValue = false;
		}
		GUI.enabled = old_gui_enabled;
		GUILayout.EndHorizontal();

		if(p_RaycastEnabled.boolValue) {
			EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastDamage"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastFalloffDamage"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastFalloffFarDamage"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastFalloffCloseDistance"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastFalloffFarDistance"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastImpactPrefab"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastImpactDuration"));
		} else {
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ProjectilePrefab"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ProjectileForce"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ProjectileForceMode"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("ProjectileOffset"));
		}

		EditorGUILayout.PropertyField(serializedObject.FindProperty("Ammo"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("MagazineSize"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("ContinuousReload"));
		EditorGUILayout.PropertyField(serializedObject.FindProperty("ReloadDuration"));
		
		serializedObject.ApplyModifiedProperties();
	}
}
