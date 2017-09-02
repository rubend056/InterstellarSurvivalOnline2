using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrailRendererCustom : MonoBehaviour{
	private LineRenderer lr;
	public float snapDistance = 1000;
	public int maxNumber = 100;
	private Vector3 lastPosition = Vector3.zero;
	private float distance = 0;
	void Start(){
		//positions = new List<Vector3> ();
		lr = gameObject.GetComponent<LineRenderer> ();
		StartCoroutine (distanceCheck ());
	}
	void customUpdate(){
		if (distance > snapDistance){
			lastPosition = gameObject.transform.position;
			if (lr.positionCount < maxNumber) {
				lr.positionCount+=1;
				Vector3[] positions = new Vector3[lr.positionCount];
				lr.GetPositions (positions);
				positions [positions.Length - 1] = lastPosition;
				lr.SetPositions (positions);
			} else {
				Vector3[] positions = new Vector3[lr.positionCount];
				lr.GetPositions (positions);
				for (int i = 0; i < positions.Length; i++) {
					if (i == positions.Length - 1) {
						positions [i] = lastPosition;
					} else
						positions [i] = positions [i + 1];
				}
				lr.SetPositions (positions);
			}
			distance = 0;
		}
	}
	private IEnumerator distanceCheck(){
		WaitForSeconds wait = new WaitForSeconds (0.5f);
		while (true) {
			yield return wait;
			distance = (gameObject.transform.position - lastPosition).sqrMagnitude;
			customUpdate ();
		}
	}
}