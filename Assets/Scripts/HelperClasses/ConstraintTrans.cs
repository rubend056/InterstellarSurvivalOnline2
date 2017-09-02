using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstraintTrans : MonoBehaviour {

	public Transform otherTrans;
	public bool pos = false;
	public bool rot = false;
	public bool scale = false;


	private Transform thisTrans;
	//private Vector3 shiftedPos = Vector3.zero;
	//private Quaternion shiftedRot = Quaternion.identity;
	// Use this for initialization
	void Start () {
		thisTrans = this.transform;
	}
	
	// Update is called once per frame
	void LateUpdate() {
		if (otherTrans != null) {
			if (pos) 
				thisTrans.position = otherTrans.position;
			if (rot) 
				thisTrans.rotation = otherTrans.rotation;
			if (scale)
				thisTrans.localScale = otherTrans.localScale;
		}
	}
}
