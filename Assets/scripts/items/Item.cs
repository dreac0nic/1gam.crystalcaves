using System.Collections;
﻿using UnityEngine;

public class Item : MonoBehaviour
{
	public bool IsOwned {
		get {
			return (Owner && Owner.HasItem(this));
		}
	}

	public Inventory Owner {
		get { return m_Owner; }
		set {
			if(m_Owner && (m_Owner.CanBeStolenFrom && m_CanBeStolen)) {
				m_Owner.Drop(this);
			}

			m_Owner = value;
		}
	}

	public bool CanBeStolen { get { return m_CanBeStolen; } }

	[SerializeField] protected Inventory m_Owner;
	[SerializeField] protected bool m_CanBeStolen;
	public string Name = "Item";
	public string Description = "You can hold it!";
	public string Slot = "general";
}
