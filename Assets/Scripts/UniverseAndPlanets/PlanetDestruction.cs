using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetDestruction : MonoBehaviour {
	public GameObject linkedObject;

	void OnCollisionEnter(Collision collision){
		if (collision.gameObject.tag == "planetRB" && collision.gameObject!=null) {
			NetTransportManager.instance.destroyObjectByID (linkedObject.GetComponent<NetIdentityCustom> ().objID);
		}
	}
}
