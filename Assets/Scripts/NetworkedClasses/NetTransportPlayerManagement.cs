using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetTransportPlayerManagement : NetTransportLights{

	public enum ListUpdateType {UpdateYourself, SyncOthers};
	public int[] allConnIDs = new int[0];
	public int[] allUniqueIDs =  new int[0];
	
	public override void ReceiveDataEvent (DataType type, PlayerInfo playerInfo, byte[] data, int index = 0){
		base.ReceiveDataEvent (type, playerInfo, data, index);

		ByteReceiver br = new ByteReceiver (data, index);
		switch (type) {
		case DataType.ListUpdate:
			switch ((ListUpdateType)br.getInt ()) {
			case ListUpdateType.UpdateYourself:
				receiveUpdateYourself (br);
				break;
			case ListUpdateType.SyncOthers:
				receiveSyncOthers (br);
				break;
			}
			break;
		case DataType.Relay:
			int[] uniqueIDs = br.getIntArray ();
			bool runInServer = br.getBool ();
			br.clean ();
			sendRelayS (br.data, playerInfo.uniqueID, uniqueIDs);
			if (runInServer)
				receiveRelayData (br, playerInfo.uniqueID);
				
			break;
		case DataType.RelayS:
			receiveRelayS (br);
			break;

		}
	}

	public void syncOthers(){
		ByteConstructor bc = new ByteConstructor ();
		bc.addLevels (new int[] { (int)DataType.ListUpdate, (int)ListUpdateType.SyncOthers });	//add types
		if (playerInfoList.Count == 1)return;
		bc.add(playerInfoList.Count-1); 		//add ammount of Players int

		byte[] standardBytes = bc.getBytes();

		for (int q = 0; q < playerInfoList.Count; q++) {					//loop for all Players
			ByteConstructor bcL = new ByteConstructor (standardBytes);

			for (int i = 0; i < playerInfoList.Count; i++)					//add all players
				if(playerInfoList[i].connID != playerInfoList[q].connID)
					bcL.add (playerInfoList [i].serialize ());

			send (bcL.getBytes(), playerInfoList[q].connID);					//send to player
		}
	}

	#region Overriten Functions

	public override void startAsClient (int offset){
		base.startAsClient (offset);
		playerInfo.name = "PlayerMe";
		playerInfo.randColor ();
	}
	public override void startAsServer (){
		base.startAsServer ();
		playerInfo.name = "Server";
		playerInfo.color = Color.white;
		playerInfo.uniqueID = 0;
	}

	public override void ClientRequestConnect (int outConnectionId){
		base.ClientRequestConnect (outConnectionId);
		PlayerInfo pi = new PlayerInfo (outConnectionId);
		pi.uniqueID = generateUniqueID ();
		pi.name = "Player" + pi.uniqueID;
		playerInfoList.Add (pi);
		updatePlayerInfoListArrays();

		updatePlayer (pi.uniqueID);
	}

	public override void ServerRequestConnect (int outConnectionId){
		base.ServerRequestConnect (outConnectionId);
		playerInfoList.Add (new PlayerInfo (outConnectionId, "Server", 0, Color.white));   //Since it's the server the unique ID will always be 0
		updatePlayerInfoListArrays();
	}

	#endregion

	#region Send Functions

	public void sendAllAdvanced(byte[] toSend){
//		DebugConsole.Log ("Check1");
		sendAllAdvanced (toSend, true, reliableChannel);
	}
	public void sendAllAdvanced(byte[] toSend, bool runInServer, int channel){
		sendRelay (toSend, runInServer, allUniqueIDs, channel);
	}

	public void sendRelay(byte[] toSend, bool runInServer, int[] uniqueIDs, int channel){
//		DebugConsole.Log ("Check2");
		if (isClient ())
			sendServer (ByteHelper.Combine (new byte[][]{
				System.BitConverter.GetBytes ((int)DataType.Relay), 
				ByteHelper.getBytes (uniqueIDs),
				ByteHelper.getBytes (runInServer),
				toSend}));
		else if (isServer())
			for (int i = 0; i < uniqueIDs.Length; i++) {
				PlayerInfo pi = playerInfoList [findWithUniqueID (uniqueIDs [i])];
				send (toSend, pi.connID);
			}

	}
	public void sendRelay(byte[] toSend, bool runInServer, int[] uniqueIDs){
		sendRelay (toSend, runInServer, uniqueIDs, reliableChannel);
	}

	public void sendRelayS(byte[] toSend, int SUniqueID, int[] DUniqueIDs, int channel){
//		DebugConsole.Log ("Check3");
		if (!isServer ())
			return;
		for (int i = 0; i < DUniqueIDs.Length; i++) {
			int index = findWithUniqueID (DUniqueIDs [i]);
			if (index > 0){
				PlayerInfo pi = playerInfoList [index];
				send (
					ByteHelper.Combine (
						System.BitConverter.GetBytes ((int)DataType.RelayS), 
						System.BitConverter.GetBytes (SUniqueID), 
						toSend),
					pi.connID
				);
			}
		}

	}
	public void sendRelayS(byte[] toSend, int SUniqueID, int[] DUniqueIDs){
		sendRelayS(toSend, SUniqueID,DUniqueIDs, reliableChannel);
	}

	public void sendAllBasic (byte[] toSend, int channel){
		if (isConnected()) {
			if (client) {
				sendServer (toSend, channel);
			} else if (server) {
				foreach (PlayerInfo playerInfo in playerInfoList) {
					send(toSend, playerInfo.connID, channel);
				}
			}
		}
	}

	public void sendAllBasic (byte[] toSend){
		sendAllBasic (toSend, reliableChannel);
	}

	public void updatePlayer(int uniqueID){
		var pi = playerInfoList [findWithUniqueID (uniqueID)];
		ByteConstructor bc = new ByteConstructor ();
		send(bc.addLevels (new int[] { (int)DataType.ListUpdate, (int)ListUpdateType.UpdateYourself }, pi.serialize()), pi.connID);
		syncOthers ();
	}
	#endregion

	#region ReceiveFunctions

	public virtual void receiveSyncOthers(ByteReceiver br){
		br.clean ();
		if (isServer ())
			return;
		//Remove all synced PlayerInfo's
		int count = 0;
		for (int i = 0; i < playerInfoList.Count; i++) {
			if (playerInfoList [count].synced) 
				playerInfoList.RemoveAt (count);
			else
				count++;
		}

		int max = br.getInt ();
		for (int i = 0; i < max; i++) {
			var pi = new PlayerInfo ();
			br.index = pi.deserialize (br.data, br.index);
			pi.synced = true;
			playerInfoList.Add (pi);
		}
		updatePlayerInfoListArrays();
	}
	public virtual void receiveUpdateYourself(ByteReceiver br){
		br.clean ();
		playerInfo.deserialize (br.data, br.index);
	}

	private void receiveRelayS(ByteReceiver br){
		DataType typeL = (DataType)br.getInt ();
		int uniqueID = br.getInt ();
		br.clean ();
		ReceiveDataEvent(typeL, playerInfoList[findWithUniqueID(uniqueID)], br.data);
	}
	private void receiveRelayData(ByteReceiver br, int uniqueID){
		DataType typeL = (DataType)br.getInt ();
		br.clean ();
		ReceiveDataEvent(typeL, playerInfoList[findWithUniqueID(uniqueID)], br.data);
	}

	#endregion

	#region PlayerInfo Handling Funcitons

	public int[] getAllUniqueIDExept(int[] exeptions){
		return GeneralHelp.deleteFromIntArray (allUniqueIDs, exeptions);
	}
	public int[] getAllConnIDExept(int[] exeptions){
		return GeneralHelp.deleteFromIntArray (allConnIDs, exeptions);
	}

	public void updatePlayerInfoListArrays (){
		allConnIDs = getAllConnIDs ();
		allUniqueIDs = getAllUniqueIDs ();
	}
	public int[] getAllConnIDs(){
		int count = playerInfoList.Count;
		int[] connIDs = new int[count];
		for (int i = 0; i < count; i++) {
			connIDs [i] = playerInfoList [i].connID;
		}
		return connIDs;
	}
	public int[] getAllUniqueIDs(){
		int count = playerInfoList.Count;
		int[] uniqueIDs = new int[count];
		for (int i = 0; i < count; i++) {
			uniqueIDs [i] = playerInfoList [i].uniqueID;
		}
		return uniqueIDs;
	}

	public void changeColor(int uniqueID, Color c){
		playerInfoList[findWithUniqueID (uniqueID)].color = c;
	}
	public void changeName(int uniqueID, string name){
		playerInfoList[findWithUniqueID (uniqueID)].name = name;
	}

	#endregion

}

[System.Serializable]
public class PlayerInfo{
	//General
	public string name;
	public int uniqueID;
	public Color color;
//	public bool me = false;
	public bool synced = false;

	//Other
	public int connID;

	public PlayerInfo(){

	}
	public PlayerInfo(bool meL, string nameL){
//		me = meL;
		name = nameL;
		randColor();
	}
	public PlayerInfo(bool meL, string nameL, Color colorL){
//		me = meL;
		name = nameL;
		color = colorL;
	}
	public PlayerInfo(int connIDL){
		connID = connIDL;
		name = "Player";
		uniqueID = 60000;
		color = Color.black;
		randColor ();
	}
	public PlayerInfo(int connIDL, string nameL){
		connID = connIDL;
		name = nameL;
		uniqueID = 60000;
		color = Color.black;
		randColor ();
	}
	public PlayerInfo(int connIDL, string nameL, int uniqueIDL){
		connID = connIDL;
		name = nameL;
		uniqueID = uniqueIDL;
		color = Color.black;
		randColor ();
	}
	public PlayerInfo(int connIDL, string nameL, int uniqueIDL, Color colorL){
		connID = connIDL;
		name = nameL;
		uniqueID = uniqueIDL;
		color = colorL;
	}
	public PlayerInfo(byte[] bytes, int index){
		deserialize (bytes, index);
	}

	public void randColor(){
		float r = 1f * Random.Range (0f,1f);
		float g = (1.5f - r) * Random.Range (0f, 1f);
		float b = 1.5f - r - g;
		if (r + g + b < 1) {
			float u = (1 - r - g - b)/3;
			r += u;
			g += u;
			b += u;
		}
		if (r > 1)
			r = 1;
		if (g > 1)
			g = 1;
		if (b > 1)
			b = 1;
		color = new Color (r, g, b, 1);
	}
	public byte[] serialize(){
		ByteConstructor bc = new ByteConstructor ();
		bc.add(System.BitConverter.GetBytes (uniqueID));
		bc.add(ByteHelper.colorBytes (color));
		bc.add(ByteHelper.getBytes(name));
		return bc.getBytes ();
	}
	public int deserialize(byte[] bytes, int index){
		ByteReceiver br = new ByteReceiver (bytes, index);
		uniqueID = br.getInt ();
		color = br.getColor ();
		name = br.getString();
		return br.index;
	}
}