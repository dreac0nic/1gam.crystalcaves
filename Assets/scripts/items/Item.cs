using System.Collections;
using System.Collections.Generic;
﻿using UnityEngine;

public class Item : MonoBehaviour
{
	public bool hasOwner {
		get {
			return Owner != null;
		}
	}

	public Inventory Owner {
		get { return m_Owner; }
		set {
			if(m_Owner != value) {
				isVisible = (value == null);
				isPhysical = (value == null);
			}

			m_Owner = value;
		}
	}

	public bool isVisible {
		get { return m_isVisible; }
		set {
			if(m_isVisible != value) {
				Debug.Log("Changing visibility to: " + value);
				foreach(Renderer rend in m_Renderers) {
					rend.enabled = value;
				}
			}

			m_isVisible = value;
		}
	}

	public bool isPhysical {
		get { return m_isPhysical; }
		set {
			if(m_isPhysical != value) {
				Debug.Log("Changing physicality to: " + value);
				if(m_Body) {
					m_Body.isKinematic = !value;
				}

				foreach(Collider coll in m_Colliders) {
					coll.enabled = value;
				}
			}

			m_isPhysical = value;
		}
	}

	public bool CanBeStolen { get { return m_CanBeStolen; } }

	[SerializeField] protected Inventory m_Owner;
	[SerializeField] protected bool m_CanBeStolen;
	public string Name = "Item";
	public string Description = "You can hold it!";
	public string Slot = "general";

	protected bool m_isVisible = true;
	protected bool m_isPhysical = true;
	protected Rigidbody m_Body;
	protected List<Renderer> m_Renderers;
	protected List<Collider> m_Colliders;

	public void Awake()
	{
		m_Body = GetComponent<Rigidbody>();
		m_Renderers = new List<Renderer>(GetComponentsInChildren<Renderer>());
		m_Colliders = new List<Collider>(GetComponentsInChildren<Collider>());

		Debug.Log(m_Body);
	}
}
