using UnityEngine;
using System.Collections;

public class CameraTransformLerper : MonoBehaviour
{
	public Transform CameraTarget;
	public bool LerpPosition = true;
	public bool SlerpRotation = true;
	public float PositionSpeed = 5.0f;
	public float RotationSpeed = 5.0f;

	public void Update()
	{
		if(CameraTarget) {
			if(LerpPosition) {
				this.transform.position = Vector3.Lerp(this.transform.position, CameraTarget.position, PositionSpeed*Time.deltaTime);
			} else {
				this.transform.position = CameraTarget.position;
			}

			if(SlerpRotation) {
				this.transform.localRotation = Quaternion.Slerp(this.transform.localRotation, CameraTarget.rotation, RotationSpeed*Time.deltaTime);
			} else {
				this.transform.localRotation = CameraTarget.rotation;
			}
		}
	}
}
