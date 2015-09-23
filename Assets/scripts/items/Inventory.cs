using System.Collections;
using System.Collections.Generic;
﻿using UnityEngine;

public class Inventory : MonoBehaviour
{
	[System.Serializable]
	public class InventorySlot
	{
		public List<Item> Items { get { return m_Items; } }

		public string test = "NOPE";
		public bool LimitItemCount = false;
		public uint MaximumItems = 2;
		public List<Transform> MountingAnchors;
		[SerializeField] protected List<Item> m_Items;

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
	public InventorySlot TestSlot;
	public Dictionary<string, InventorySlot> Slots;
	public List<InventorySlot> TestSlots;
}
