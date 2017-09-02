using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRotator : MonoBehaviour {

	public Transform planetTransform;
	public Vector3 rotationSpeed = Vector3.zero;
	private Vector3 rotation;
	public bool smoothOnStart = true;
	public bool smooth = true;
	public float smoothTime = 2;
	private Vector3 eulerAngles;
	// Use this for initialization
	void Start () {
		if (!planetTransform)
			planetTransform = gameObject.transform;
		if (planetTransform!=null)
			rotation = planetTransform.eulerAngles;
		eulerAngles = rotation;

		if (smoothOnStart) {
			Vector3 rotSpeed = rotationSpeed;
			rotationSpeed = Vector3.zero;
			changeSpeed (rotSpeed);
		}
	}


	public void restart(){
		planetTransform.eulerAngles = eulerAngles;
		rotationSpeed = Vector3.zero;
		StopAllCoroutines ();
	}
	// Update is called once per frame
	void Update () {
		if (planetTransform && rotationSpeed!=Vector3.zero){
			rotation += Time.deltaTime * 60 * rotationSpeed;
			planetTransform.eulerAngles = rotation;
		}
	}

	public void changeSpeed(Vector3 speed, float interpolation = 2){
		if (smooth)
			changeRotSpeed (speed,interpolation);
		else {
			if (coroutine2 != null)
				StopCoroutine (coroutine2);
			rotationSpeed = speed;
		}
	}

	private Coroutine coroutine2;
	void changeRotSpeed(Vector3 speed, float interpolation = 2){
		if (coroutine2 != null)
			StopCoroutine (coroutine2);
		coroutine2 = StartCoroutine (changeRotSpeedCoroutine (speed,interpolation));
	}

	private IEnumerator changeRotSpeedCoroutine( Vector3 speed, float interpolation){
		while(rotationSpeed != speed){
			//Vector3 rotSpeed = rotationSpeed;
			rotationSpeed = Vector3.MoveTowards (rotationSpeed, speed,  Time.deltaTime *(Mathf.Abs(Mathf.Sqrt(speed.sqrMagnitude-rotationSpeed.sqrMagnitude))/interpolation));
			yield return null;
		}
	}
}
