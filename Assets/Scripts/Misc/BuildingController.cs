using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingController : MonoBehaviour {

	private Transform buildingTransform;
	private Vector3 fullSize;
	public float cost = 15;
	public GameObject toCreate;
	public Transform planet;
	public float size;
	private float lumberToSize;
	// Use this for initialization
	void Start () {
		gameObject.tag = "BuildingUnfinished";
		buildingTransform = gameObject.transform;
		fullSize = gameObject.transform.localScale;
		gameObject.transform.localScale = new Vector3 (fullSize.x, fullSize.y / cost, fullSize.z);
		size = fullSize.y / cost;
		lumberToSize = size;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
		
	public bool build(float ammount){
		if ((lumberToSize * ammount) + size > fullSize.y) {
			size = fullSize.y;
			updateSize (size);
			return true;
		} else {
			size += (lumberToSize * ammount);
			updateSize (size);
			return false;
		}

	}

	void updateSize(float size){
		buildingTransform.localScale = new Vector3 (fullSize.x, size, fullSize.z);

		if (isComplete ()) gameObject.tag = "Building";
		else gameObject.tag = "BuildingUnfinished";
	}

	bool isComplete(){
		return (size == fullSize.y);
	}

//	public void spawnObject(){
//		if (toCreate != null) {
//			GameObject pesantInstance = GameObject.Instantiate (toCreate) as GameObject;
//			pesantInstance.transform.position = buildingTransform.position;
//			pesantInstance.GetComponent<PesantAI> ().planet = planet;
//		}
//	}

}
