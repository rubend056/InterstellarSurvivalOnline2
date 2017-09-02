using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetTransportLights : NetTransportManager {

	[Space(3)]
	[Header("Lights")]
	public GameObject sendLightGO;
	public GameObject receiveLightGO, updateLightGO, createLightGO;

	private Coroutine sendLightC, receiveLightC, updateLightC, createLightC;

	public override void Start (){
		base.Start ();
		sendLightGO.SetActive (false);
		receiveLightGO.SetActive (false);
		updateLightGO.SetActive (false);
		createLightGO.SetActive (false);
	}

	public override void lowSendCommand (int socket, int connID, int channel, byte[] toSend, int length, out byte error){
		base.lowSendCommand (socket, connID, channel, toSend, length, out error);
		sendLightEnable ();
	}
	public override void ReceiveDataEvent (DataType type, PlayerInfo playerInfo, byte[] data, int index = 0){
		base.ReceiveDataEvent (type, playerInfo, data, index);
		receiveLightEnable ();
	}

	public void sendLightEnable(){
		if (sendLightGO == null)
			return;
		if (sendLightC != null)
			StopCoroutine (sendLightC);
		sendLightC = StartCoroutine(wait(sendLightGO));
	}
	public void receiveLightEnable(){
		if (receiveLightGO == null)
			return;
		if (receiveLightC != null)
			StopCoroutine (receiveLightC);
		receiveLightC = StartCoroutine(wait(receiveLightGO));
	}

	public void updateLightEnable(){
		if (updateLightGO == null)
			return;
		if (updateLightC != null)
			StopCoroutine (updateLightC);
		updateLightC = StartCoroutine(wait(updateLightGO));
	}
	public void createLightEnable(){
		if (createLightGO == null)
			return;
		if (createLightC != null)
			StopCoroutine (createLightC);
		createLightC = StartCoroutine(wait(createLightGO));
	}
	private IEnumerator wait(GameObject gO){
		gO.SetActive (true);
		yield return null;
		yield return null;
		gO.SetActive (false);
	}
}
