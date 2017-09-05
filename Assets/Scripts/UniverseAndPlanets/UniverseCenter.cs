using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UniverseCenter : MonoBehaviour{
	public static UniverseCenter instance;

	public Transform universeCenter;

	public bool centerUniverse = false;
	public float snapDistance = 1000;
	public Vector3 shiftedPos =  Vector3.zero;

	public List<Transform> allOthers;
	public LineRenderer[] trenderers;

	void Awake(){
		instance = this;
		allOthers = new List<Transform> ();
		trenderers = new LineRenderer[0];
	}

	void Start(){
		switchCenter ();
	}

	//private int i = 0;
	void Update(){
		if (centerUniverse && universeCenter != null) {
			
			if (universeCenter.position.sqrMagnitude > snapDistance) {
				shiftedPos += universeCenter.position;
				//hiftedRot += universeCenter.rotation.eulerAngles;
				shiftEverything (-universeCenter.position);
			}
		}
	}

	void OnCenterOn(){
		UpdateAllOthers ();
		shiftedPos = universeCenter.position;
		shiftEverything (-universeCenter.position);

	}
	void OnCenterOff(){
		
		shiftEverything(shiftedPos);
	}

	void UpdateAllOthers(){
		allOthers.Clear ();
		GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>() ;
		for (int i = 0; i < allObjects.Length; i++) {
			if (allObjects[i].transform.parent == null && 
				allObjects[i].layer != 5 &&
				allObjects[i].tag != "Singleton" &&
				allObjects[i].tag != "UI") {
				allOthers.Add (allObjects [i].transform);
			}
		}
		List<LineRenderer> lrList = new List<LineRenderer> ();
		foreach (Transform trans in allOthers) {
			var trail = trans.GetComponent<LineRenderer> ();
			if (trail != null)
				lrList.Add (trail);
		}
		trenderers = lrList.ToArray ();
	}

	void shiftEverything(Vector3 ammount/*, Vector3 rotammount*/){

		shiftedPos -= ammount;

		foreach (Transform trans in allOthers) {
			trans.position += ammount;
		}
		//master.transform.eulerAngles += rotammount;

		for (int i = 0; i < trenderers.Length; i++) {
			Vector3[] somethings = new Vector3[trenderers[i].positionCount];trenderers[i].GetPositions(somethings);
			for (int e = 0; e < somethings.Length; e++) {
				somethings [e] += ammount;
			}
			trenderers[i].SetPositions(somethings);
		}
	}

	public void switchCenter(){
		if (universeCenter == null)
			return;
		centerUniverse = !centerUniverse;
		if (centerUniverse)
			OnCenterOn ();
		else
			OnCenterOff ();
	}

	public bool isCenter(){
		return centerUniverse;
	}
	void OnValidate(){switchCenter ();}
}