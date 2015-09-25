using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Item))]
public class InventoryPickupTarget : FireTargetBase
{
	protected Item m_Item;

	public void Awake()
	{
		m_Item = this.GetComponent<Item>();
	}
	public override void Fire(GameObject offender, TriggerType type = TriggerType.GENERAL)
	{
		Inventory possible_owner = offender.GetComponent<Inventory>();

		if(possible_owner) {
			possible_owner.Pickup(m_Item);
		}
	}
}
