using UnityEngine;
using System.Collections;

public class WeaponImpulser : Impulser
{
	public Animator WeaponAnimator;

	protected Shootable m_Weapon;

	public void Awake()
	{
		m_Weapon = GetComponent<Shootable>();
	}

	public override void Impulse(ImpulseType type)
	{
		if(!m_Weapon) {
			m_Weapon = GetComponent<Shootable>();
		}

		if(m_Weapon) {
			Item weapon_item = GetComponent<Item>();
			FirstPersonMovementController movement_controller = (weapon_item && weapon_item.Owner ? weapon_item.Owner.GetComponent<FirstPersonMovementController>() : null);
			Transform camera_anchor = (movement_controller ? movement_controller.CameraAnchor : null);

			switch(type) {
				case ImpulseType.ATTACK:
					m_Weapon.Fire(camera_anchor);
					break;

				case ImpulseType.REFRESH:
					m_Weapon.Reload();
					break;

				default:
					base.Impulse(type);
					break;
			}
		}
	}
}
