//using System.Collections;
//using System.Collections.Generic;
//using System;
//using UnityEngine;
//public class NetHealth : MonoBehaviour{
//	public static NetHealth instance;
//	private enum CmdType{Damage, HealthSync};
//
//	void Awake(){
//		instance = this;
//	}
//	public void onReceived(byte[] data){
//		ByteReceiver br = new ByteReceiver (data);
//		CmdType type = (CmdType)br.getInt ();
//		int objID = br.getInt ();
//
//		switch (type) {
//		case CmdType.Damage:
//			float damage = br.getFloat ();
//			onDamage(objID, damage);
//			break;
//		case CmdType.HealthSync:
//			float health = br.getFloat ();
//			receiveSyncHealth (objID, health);
//			break;
//		}
//	}
//
//	public void doDamage(int objID, float damage){
//		ByteContructor bc = new ByteContructor();
//		bc.add((int)NetObjectSync.DataType.Health);
//		bc.add((int)CmdType.Damage);
//		bc.add(objID);
//		bc.add(damage);
//		NetTransportManager.instance.sendToConnected (data.ToArray());
//	}
//	private void onDamage(int objID, float damage){
//		GameObject gO = NetObjectSync.instance.findGameObjectByID (objID);
//		var controller =  gO.GetComponent<HealthController> ();
//		controller.receiveDamage (damage);
//		if (NetTransportManager.instance.isServer ())
//			syncAllHealth (objID, controller.health);
//	}
////	public void syncAllHealth(int objID, float health){
////		ByteContructor bc = new ByteContructor();
////		bc.add((int)NetObjectSync.DataType.Health);
////		bc.add((int)CmdType.Damage);
////		bc.add(objID);
////		bc.add(health);
////		NetTransportManager.instance.sendToConnected (data.ToArray());
////	}
//	public void sendSyncHealth(int objID, float health){
//		List<byte> data = new List<byte> ();
//		br.add((int)NetObjectSync.DataType.Health);
//		br.add((int)CmdType.HealthSync);
//
//	}
//	public void receiveSyncHealth(int objID, float health){
//		NetObjectSync.instance.findGameObjectByID (objID).GetComponent<HealthController> ().health = health;
//	}
//	
//}