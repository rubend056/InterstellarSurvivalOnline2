using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropDownUI : MonoBehaviour {

	public int maxItems = 50;
	List<GameObject> items;
	// Use this for initialization
	void Start () {
		items = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public bool addItem (GameObject toAdd){
		return true;
	}
	public void toggle(){
		
	}
		
}
