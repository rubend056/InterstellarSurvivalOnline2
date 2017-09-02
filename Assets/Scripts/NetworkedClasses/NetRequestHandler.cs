//using UnityEngine;
//using System;
//using System.Collections.Generic;
//using System.Text;
//
//public class NetRequestHandler : MonoBehaviour{
//	public static NetRequestHandler instance;
//
//	public enum RequestType{Update, AuthorityChange, PlayerListUpdate};
//	public enum ListRequestType{Add, Remove, Clear};
//
//	void Awake(){
//		instance = this;
//	}
//
//	public void receiveRequest(byte[] data, int connID){
//		ByteReceiver receiver = new ByteReceiver (data);
//		RequestType type = (RequestType)receiver.getInt ();
//		switch (type) {
//		case RequestType.Update:
//			int objectID = receiver.getInt ();
//			if (objectID >= 0)
//				NetTransportObjectSync.instance.sendData(NetTransportObjectSync.DataType.Update, NetTransportObjectSync.instance.findIndexByID(objectID), connID,NetTransportManager.reliableChannel);
//			else 
//				NetTransportObjectSync.instance.sendUniverseDataUpdate(connID);
//			break;
//		case RequestType.PlayerListUpdate:
//			receivePlayerListUpdate(data, receiver.index);
//			break;
//		case RequestType.AuthorityChange:
//			int objectID1 = receiver.getInt ();
//			int playerID = receiver.getInt ();
//			NetTransportObjectSync.instance.changeAuthority (objectID1, playerID);
//			break;
//		}
//	}
//
//	private void receivePlayerListUpdate(byte[] data, int indexCount){
//		ByteReceiver receiver = new ByteReceiver (data, indexCount);
//		var listUpType = (ListRequestType)receiver.getInt ();
//		int uniqueID = receiver.getInt ();
//		int nameLength = receiver.getInt ();
//		string name = receiver.getString (nameLength);
//
//		NetTransportManager.instance.updatePlayerList (listUpType, uniqueID, name);
//	}
//
//	public bool sendPlayerListUpdate(ListRequestType type, int playerID, string name){
//		if (NetTransportManager.instance.isServer()) {
//			List<byte> data = new List<byte>();
//			data.AddRange (BitConverter.GetBytes ((int)NetTransportObjectSync.DataType.Request));
//			data.AddRange (BitConverter.GetBytes ((int)RequestType.PlayerListUpdate));
//			data.AddRange (BitConverter.GetBytes ((int)type));
//			data.AddRange (BitConverter.GetBytes (playerID));
//			data.AddRange (BitConverter.GetBytes (name.Length));
//			data.AddRange (Encoding.Unicode.GetBytes(name));
//			NetTransportManager.instance.sendToConnected (data.ToArray(), NetTransportManager.reliableChannel);
//			return true;
//		} else
//			return false;
//	}
//
//	public bool sendAuthorityChangeRequest(int objectID, int playerID){
//		if (NetTransportManager.instance.isClient ()) {
//			List<byte> data = new List<byte>();
//			data.AddRange (BitConverter.GetBytes ((int)NetTransportObjectSync.DataType.Request));
//			data.AddRange (BitConverter.GetBytes ((int)RequestType.AuthorityChange));
//			data.AddRange (BitConverter.GetBytes (objectID));
//			data.AddRange (BitConverter.GetBytes (playerID));
//			NetTransportManager.instance.sendToConnected (data.ToArray(), NetTransportManager.reliableChannel);
//			GameObject there;
//			return true;
//		} else
//			return false;
//	}
//
//	public bool sendUpdateRequest(int objectID){
//		if (NetTransportManager.instance.isClient ()) {
//			List<byte> data = new List<byte>();
//			data.AddRange (BitConverter.GetBytes ((int)NetTransportObjectSync.DataType.Request));
//			data.AddRange (BitConverter.GetBytes ((int)RequestType.Update));
//			data.AddRange (BitConverter.GetBytes (objectID));
//			NetTransportManager.instance.sendToConnected (data.ToArray(), NetTransportManager.reliableChannel);
//			return true;
//		} else
//			return false;
//	}
//
//	//public bool sendCustomRequest( )
//
//}