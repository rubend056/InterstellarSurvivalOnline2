using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInstanceInfo : MonoBehaviour {

	public Animator anim;
	public Rigidbody ownRigidBody;
	public Transform ownTransform;
	public Transform torsoTransform;
	public Transform bulletSpawn;
	public Text nameText;
	public NetIdentityCustom ni;
	public GameObject healthBar = null;
	public Transform gunTrans;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
