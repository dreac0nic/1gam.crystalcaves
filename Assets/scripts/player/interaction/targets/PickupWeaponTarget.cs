using UnityEngine;
using System.Collections;

//[RequireComponent(typeof (Shootable))]
public class PickupWeaponTarget : FireTargetBase
{
	// protected Shootable m_Weapon;

	void Awake()
	{
		// m_Weapon = GetComponent<Shootable>();
	}

	public override void Fire(GameObject offender, TriggerType type = TriggerType.GENERAL)
	{
		/*
		WeaponInventory offenderInventory = offender.GetComponent<WeaponInventory>();

		if(offenderInventory)
			offenderInventory.PickupWeapon(m_Weapon);
		//*/
	}
}
