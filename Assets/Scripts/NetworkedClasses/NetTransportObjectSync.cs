using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NetTransportObjectSync : NetTransportText{

	public enum SyncType{Spawning, TransUpdate, Request};
	public enum TransUpdateType{Transform, /*Position, Rotation, Scale, Rigidbody,*/ TransAndRigidbody};
	public enum RequestType{Universe};

	public int genIDIndex = 0;

	[Space(5)]
	[Header("NetObjectSync")]
	public List<GameObject> spawnableObjects;
	public List<IdentityAndTransform> IDAndTransforms;
	public List<Coroutine> coroutines;

	public override void Start (){
		base.Start ();
		spawnableObjects = new List<GameObject> ();
		IDAndTransforms = new List<IdentityAndTransform> ();
		coroutines = new List<Coroutine> ();
	}

	public override void ReceiveDataEvent (DataType type, PlayerInfo playerInfo, byte[] data, int index = 0){
		base.ReceiveDataEvent (type, playerInfo, data, index);

		if (type == DataType.Sync) {
			ByteReceiver br = new ByteReceiver (data, index);
			var stype = (SyncType)br.getInt ();
			data = br.clean();
			switch(stype){
			case SyncType.Spawning:
				receiveSpawn (data);
				break;
			case SyncType.TransUpdate:
				receiveTransUpdate (data);
				break;
			case SyncType.Request:
				if (!server) {
					logMessage ("Request Received, not server", "error");
					return;
				}
				receiveRequest (data, playerInfo);
				break;
			}
		}
	}

	#region SendFunctions

	private byte[] getSpawnBytes (IdentityAndTransform iat){ // list index means index in the IDAndTransforms list, and conn
		ByteConstructor bc = new ByteConstructor ();
		bc.addLevels (new int[]{(int)DataType.Sync, (int)SyncType.Spawning});
		bc.add (NetObjBytes.defaultInfo (iat.netIdentity.objID, iat.netIdentity.AuthorityID, (int)iat.type, iat.prefabIndex, iat.netTrans.trans.position, iat.netTrans.trans.rotation));
		switch (iat.type) {
		case NetObjBytes.ObjectType.Planet:
			var pg3 = iat.netIdentity.GetComponent<PlanetGenerator3> ();

			if (pg3 != null) {
				var rb = iat.netIdentity.GetComponent<ConstraintTrans> ().otherTrans.GetComponent<Rigidbody> ();
				bc.add (NetObjBytes.planetSpawn (rb.mass, rb.velocity, rb.angularVelocity, pg3.seed, pg3.radius));
			} else {
				var rb = iat.netIdentity.GetComponent<Rigidbody> ();
				bc.add (NetObjBytes.planetSpawn (rb.mass, rb.velocity, rb.angularVelocity, 0, 0));
			}
			break;
		case NetObjBytes.ObjectType.Rigidbody:
			var rb1 = iat.netIdentity.GetComponent<ConstraintTrans> ().otherTrans.GetComponent<Rigidbody> ();
			bc.add (NetObjBytes.rigidbodySpawn (rb1.mass, rb1.velocity, rb1.angularVelocity));
			break;
		}

		return bc.getBytes ();
	}

//	private byte[] transUpdateBytes(IdentityAndTransform iat){
//		return transUpdateBytes (iat.netTrans.trans, iat.netTrans.updateType, iat.netIdentity.objID);
//	}
	private byte[] transUpdateBytes(IdentityAndTransform iat){
		ByteConstructor bc = new ByteConstructor ();
		bc.addLevels(new int[]{(int)DataType.Sync,(int)SyncType.TransUpdate});

		bc.add(iat.netIdentity.objID);
		bc.add((int)iat.netTrans.updateType);
		switch(iat.netTrans.updateType){
		case TransUpdateType.Transform:
			bc.add (iat.netTrans.getPosition());
			bc.add (iat.netTrans.getRotation());
			bc.add (transform.localScale);
			break;
//		case TransUpdateType.Position:
//			bc.add (ByteHelper.vector3Bytes(transform.position));
//			break;
//		case TransUpdateType.Rotation:
//			bc.add (ByteHelper.quaternionBytes(transform.rotation));
//			break;
//		case TransUpdateType.Scale:
//			
//			break;
		case TransUpdateType.TransAndRigidbody:
			bc.add (iat.netTrans.getPosition());
			bc.add (iat.netTrans.getRotation());
			bc.add (transform.localScale);

			var body = transform.GetComponent<Rigidbody> ();
			bc.add (body.velocity);
			bc.add (body.angularVelocity);
			break;
		default:
			break;
		}
		return bc.getBytes ();
	}
	public void sendUniverseUpdateRequest(){
		ByteConstructor bc = new ByteConstructor (12);
		bc.addLevels (new int[]{(int)DataType.Sync, (int)SyncType.Request, (int)RequestType.Universe});
		sendServer (bc.getBytes ());
	}
	#endregion

	#region ReceiveFunctions
	private void receiveTransUpdate(byte[] data){
		receiveLightEnable ();

		ByteReceiver br = new ByteReceiver (data);
		int objID = br.getInt ();
		var updateType = (TransUpdateType)br.getInt ();

		int transIndex = findIndexByID (objID);

		if (transIndex != -1) {
			var netTrans = IDAndTransforms [transIndex].netTrans;
			switch (updateType) {
			case TransUpdateType.Transform:
				netTrans.receiveTransform (br);
				break;
//			case TransUpdateType.Position:
//				IDAndTransforms[transIndex].netTrans .moveTo(br.getVector3 ());
//				break;
//			case TransUpdateType.Rotation:
//				IDAndTransforms[transIndex].netTrans .rotateTo(br.getQuaternion ());
//				break;
//			case TransUpdateType.Scale:
//				IDAndTransforms[transIndex].netTrans .trans.localScale = br.getVector3 ();
//				break;
			case TransUpdateType.TransAndRigidbody:
				netTrans.receiveTransform (br);
				var body = netTrans.trans.GetComponent<Rigidbody> ();
				body.velocity = br.getVector3 ();
				body.angularVelocity = br.getVector3 ();
				break;
			}
		} else {
			logMessage ("ObjectNotFound ID:" + objID, "error");
		}
	}

	private void receiveSpawn(byte[] data){ // Receives a spawn request
		createLightEnable();

		int objID = System.BitConverter.ToInt32 (data, 0); //Converting data to int
		int indexOfFound = findIndexByID (objID); // try and find an object with the same netID
		if (indexOfFound != -1) // if object was found, destroy it
			destroyObject(indexOfFound);

		IdentityAndTransform idTransLocal;
		NetObjBytes.receiveObject (data, out idTransLocal); //Hand all of the spawning data to the NetObjSpawn Class function "receiveObject"
		IDAndTransforms.Add (idTransLocal);
	}

	private void receiveRequest(byte[] data, PlayerInfo pi){
		ByteReceiver br = new ByteReceiver (data);

		switch ((RequestType)br.getInt ()) {
		case RequestType.Universe:
			sendUniverseDataUpdate (pi.connID);
			break;
		}

	}
	#endregion

	#region Generic Local Functions
	public int findIndexByID( int objID){
		for (int i = 0; i < IDAndTransforms.Count; i++) {
			if (IDAndTransforms [i].netIdentity.objID == objID)
				return i;
		}
		return -1;
	}
	public GameObject findGameObjectByID(int objID){
		int index = findIndexByID (objID);
		if (index != -1)
			return IDAndTransforms [index].netTrans.gameObject;
		else
			return null;
	}
	public void destroyObject(int index){
		if (index < 0)
			return;
		if (IDAndTransforms [index].type == NetObjBytes.ObjectType.Planet) {
			if (IDAndTransforms [index].netIdentity.GetComponent<PlanetGenerator3> () != null) {
				GameObject.Destroy (IDAndTransforms [index].netIdentity.GetComponent<ConstraintTrans> ().otherTrans.gameObject);
			}
			UniverseManager.instance.removePlanet (IDAndTransforms [index].netIdentity.objID);
		}
		GameObject.Destroy (IDAndTransforms [index].netTrans.gameObject);
		IDAndTransforms.RemoveAt (index);
	}
	public void destroyObjectByID(int objID){
		destroyObject (findIndexByID (objID));
	}

	public void sendUniverseDataBegin(int connID){
		for(int i = 0;i<IDAndTransforms.Count;i++) {
			if (IDAndTransforms[i].child == false)
				send (getSpawnBytes (IDAndTransforms [i]), connID, reliableChannel);
		}
	}
	public void sendUniverseDataUpdate(int connID){
		for(int i =0;i<IDAndTransforms.Count;i++) {
			send (transUpdateBytes (IDAndTransforms [i]), connID, reliableChannel);
		}
	}

	public void changeAuthority(int objID, int uniqueID){
		int objIndex = findIndexByID (objID);
		if (objIndex != -1)
			IDAndTransforms [objIndex].netIdentity.AuthorityID = uniqueID;
	}

	private IEnumerator syncCoroutine(IdentityAndTransform iat){
		
		WaitForSeconds w4s = new WaitForSeconds (4f);
		while (iat != null) {
			if (iat.netTrans.sendRate > 0 && iat.netIdentity.HasAuthority && isConnected ()) {
				sendAllAdvanced(transUpdateBytes (iat));
				yield return new WaitForSeconds (1f/iat.netTrans.sendRate);
			}else
				yield return w4s;
		}
	}

	public int generateID(){
		return genIDIndex++;
	}
	#endregion

	#region Server And Client Start Functions + StopAll Function
	public override void startAsServer (){
		base.startAsServer ();
	}
	public override void ClientRequestConnect (int connID){
		base.ClientRequestConnect (connID);
		sendUniverseDataBegin (connID);
	}
	public override void StopAllConnections (bool serverRequested){
		base.StopAllConnections (serverRequested);

	}
	#endregion

	#region NetObjSpawn Functions

	public IdentityAndTransform spawnObjectSync(int prefabIndex, int authorityID, Vector3 pos, Quaternion rot, int objectID){
		destroyObjectByID (objectID);

		GameObject rootObj = GameObject.FindGameObjectWithTag ("root");
		var instance = GameObject.Instantiate (spawnableObjects [prefabIndex], pos, rot);
		if (rootObj != null)
			instance.transform.parent = rootObj.transform;

		var iat = new IdentityAndTransform (instance, prefabIndex, objectID, authorityID, false);

		IDAndTransforms.Add (iat);
		coroutines.Add (StartCoroutine(syncCoroutine(iat)));

		foreach (GameObject gO in GeneralHelp.getAllChildren(instance.transform)) {
			var netIdent = gO.GetComponent<NetIdentityCustom> ();
			if (netIdent != null)
				IDAndTransforms.Add (new IdentityAndTransform (gO, prefabIndex, ++objectID, authorityID, true));
		}

		return iat;
	}
	public IdentityAndTransform spawnObject(int prefabIndex, int authorityID, Vector3 pos, Quaternion rot){
		if (!server)
			return null;

		GameObject rootObj = GameObject.FindGameObjectWithTag ("root");
		var instance = GameObject.Instantiate (spawnableObjects [prefabIndex], pos, rot);
		if (rootObj != null)
			instance.transform.parent = rootObj.transform;

		var iat = new IdentityAndTransform (instance, prefabIndex, generateID(), authorityID, false);

		IDAndTransforms.Add (iat);
		coroutines.Add (StartCoroutine(syncCoroutine(iat)));

		foreach (GameObject gO in GeneralHelp.getAllChildren(instance.transform)) {
			var netIdent = gO.GetComponent<NetIdentityCustom> ();
			if (netIdent != null)
				IDAndTransforms.Add (new IdentityAndTransform (gO, prefabIndex, generateID (), authorityID, true));
		}

		return iat;
	}
//	public int findSpawnableIndex(GameObject gO){
//		for (int i = 0; i < spawnableObjects.Count; i++) {
//			if (spawnableObjects [i] == gO)
//				return i;
//		}
//		return -1;
//	}

	#endregion

}

[System.Serializable]
public class IdentityAndTransform{
	public GameObject instance;
	public NetObjBytes.ObjectType type;
	public int prefabIndex = 0;
	public NetIdentityCustom netIdentity;
	public NetTransformCustom netTrans;
	public bool child = false;

	public IdentityAndTransform(GameObject instanceL, int prefabIndexL, int objID, int authorityID, bool childL){
		instance = instanceL;
		prefabIndex = prefabIndexL;
		child = childL;

		netIdentity = instanceL.GetComponent <NetIdentityCustom>();
		if (netIdentity == null) 
			netIdentity = instanceL.AddComponent<NetIdentityCustom> ();
		
		type = netIdentity.type;	
		netIdentity.objID = objID;
		netIdentity.AuthorityID = authorityID;
		
		netTrans = instanceL.GetComponent <NetTransformCustom>();
		if (netTrans == null)
			netTrans = instanceL.AddComponent<NetTransformCustom> ();

	}

}
