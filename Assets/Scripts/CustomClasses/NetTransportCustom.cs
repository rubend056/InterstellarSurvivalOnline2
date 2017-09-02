using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetTransportCustom : NetTransportObjectSync{

	[Space(3)]
	[Header("Custom")]
	public GameObject spawnMenu;
	public GameObject loginMenu;
//	public Slider speedSlider;

	void Awake(){
		instance = this;
	}
	public override void startAsServer (){
		base.startAsServer ();
		UniverseManager.instance.CreatePlanets ();
	}
	public override void Update (){
		base.Update ();
		if (Input.GetKey (KeyCode.RightShift) && Input.GetKeyDown (KeyCode.P)) {
			StopAllConnections (false);
		}
	}
	public override void ConnStateChangeEvent (bool state){
		base.ConnStateChangeEvent (state);
		loginMenu.SetActive (!state);
		spawnMenu.SetActive (state);
	}

	public void togglePlayerSpawner(bool value){
		spawnMenu.SetActive (value);
	}

//	public void speedChanged(){
//		Time.timeScale = speedSlider.value;
//	}
}
