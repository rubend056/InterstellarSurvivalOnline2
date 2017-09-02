using UnityEngine;
using System;

public class PlanetOrbitTranslator : MonoBehaviour{
	public Transform trans;
	public Transform center;
	public Vector3 axis = Vector3.up;
	public float radiusSpeed = 0.5f;
	public float rotationSpeed = 10000f;
	private float radius = 0;
	private bool rotate = false;
	private float movementSpeed;
	void Start(){
		if (!trans)
			trans = gameObject.transform;
		
	}
	void Update(){
		if (rotate) {
			trans.RotateAround (center.position, axis, movementSpeed * Time.deltaTime);
			//var desiredPosition = (trans.position - center.position).normalized * radius + center.position;
			//trans.position = Vector3.MoveTowards (trans.position, desiredPosition, Time.deltaTime * radiusSpeed);
		}
	}

	public void updateRadius(){
		radius = Vector3.Distance (trans.position, center.position);
		movementSpeed = rotationSpeed/radius;
		rotate = true;
	}
	void OnValidate(){
		updateRadius ();
	}
}

