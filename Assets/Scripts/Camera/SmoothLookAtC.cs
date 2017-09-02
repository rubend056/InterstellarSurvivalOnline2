using UnityEngine;
using System.Collections;

public class SmoothLookAtC : MonoBehaviour{

	public CameraControlAdva.ViewMode viewMode = CameraControlAdva.ViewMode.definitePos;
	public Transform target;
	public bool useOtherOrient = false;
	public Transform planet;
	public Transform otherOrientation;
	public float damping = 6.0f;
	public float YOffset = -0.2f;
	public bool smooth = true;

	//private Vector3 definiteLocalRotation;
	void Update () {
		if (viewMode == CameraControlAdva.ViewMode.definitePos) {

			Quaternion rotation = Quaternion.identity;
			Quaternion rotationToSet;

			rotationToSet = Quaternion.Slerp (transform.localRotation, rotation, Time.unscaledDeltaTime * damping);
			transform.localRotation = rotationToSet;

		}else if (target != null && viewMode == CameraControlAdva.ViewMode.around) {

			Vector3 toSet = target.position;

			// Look at and dampen the rotation
			Quaternion rotation = Quaternion.identity, rotationToSet;
			if (planet != null)
				toSet += ((planet.position - target.position).normalized * YOffset);

			if (useOtherOrient && otherOrientation != null)
				rotation = Quaternion.LookRotation (toSet - transform.position, otherOrientation.up);
			else
				rotation = Quaternion.LookRotation (toSet - transform.position);

			if (smooth)
				rotationToSet = Quaternion.Slerp (transform.rotation, rotation, Time.unscaledDeltaTime * damping);
			else
				rotationToSet = rotation;

			gameObject.transform.rotation = rotationToSet;
		}
	}

	void Start () {
		if (target == null) {
			if (gameObject.name == "HealthCanvas")
				target = GameObject.FindGameObjectWithTag ("MainCamera").transform;
			//else target = GameObject.FindGameObjectWithTag ("planet").transform;
		}

//		if (planet == null)
//			planet = GameObject.FindGameObjectWithTag ("planet").transform;

		//definiteLocalRotation = transform.localEulerAngles;
	}

}