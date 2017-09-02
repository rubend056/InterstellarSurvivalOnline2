using UnityEngine;
using System.Collections;

public class LookAtScript : MonoBehaviour {

	// Use this for initialization
	public float acceleration = 2;
	Transform myself;

	void Start () {
		
		myself = gameObject.transform;
		myself.eulerAngles = new Vector3 (0, 0, 0);
	}
	
	// Update is called once per frame
	void Update () {
		float forceX = Input.GetAxis("Mouse X");
		//float forceY = Input.GetAxis ("Mouse Y");

		myself.Rotate (/*forceY * acceleration*/0, forceX * acceleration, 0);
	}
}
