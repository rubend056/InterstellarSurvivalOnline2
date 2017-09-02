using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour {


	public List<thing> things;


	void Start(){
		things = new List<thing> ();
	}

	public void addObject(GameObject toAdd){
		int nameIndex = checkForName (things, toAdd.tag);
		if (nameIndex >= 0)
			things [nameIndex].count++;
		else {
			thing tInst = new thing();
			tInst.name = toAdd.tag;
			tInst.gO = GameObject.Instantiate(toAdd);
			tInst.gO.SetActive(false);
			tInst.count = 1;
			things.Add (tInst);
		}
	}

	public bool removeObject(GameObject toRemove){
		int nameIndex = checkForName (things, toRemove.tag);
		if (nameIndex >= 0) {
			things [nameIndex].count--;
			if (things [nameIndex].count == 0) {
				GameObject.Destroy (things [nameIndex].gO);
				things.RemoveAt (nameIndex);
			}
			return true;
		}
		else return false;
		
	}

	public static int checkForName(List<thing> list,string toCheck){
		for (int i = 0; i < list.Count; i++) {
			if (list [i].name == toCheck)
				return i;
		}
		return -1;
	}


}

[System.Serializable]
public class thing{
	public string name;
	public GameObject gO;
	public int count;
	public int cost;
}