using System;
using System.Collections;
using UnityEngine;

public class NetTransportDebug : NetTransportPlayerManagement{

	public override void NetErrorEvent (byte error){
		base.NetErrorEvent (error);
		var netError = (UnityEngine.Networking.NetworkError)error;
		if (netError != UnityEngine.Networking.NetworkError.Ok)
			logMessage (netError.ToString(), "error");
	}

	public override void startAsServer(){
		base.startAsServer ();
		logMessage ("Server Created");
		string ip = getLocalIP ();
		logMessage (String.Format("Your IP is: {0}", ip), "warning");
	}
	public override void startAsClient(int offset){
		base.startAsClient (offset);
		logMessage ("Client Created");
	}

	public override void ClientRequestConnect (int connID){
		base.ClientRequestConnect (connID);
		logMessage (String.Format( "NewClientConnected: {0}", playerInfoList[findWithConnID(connID)].name));
	}

	public override void InitNetwork (){
		base.InitNetwork ();
		logMessage ("NetworkInitialized");
	}

	public override void Connect (){
		logMessage ("Connecting...");
		base.Connect ();
	}
	public override void ReceiveConnAffirmation (){
		base.ReceiveConnAffirmation ();
		logMessage ("Connected");
	}
	public override void ServerRequestDisconnect (){
		base.ServerRequestDisconnect ();
		logMessage ("ServerDown", "error");
	}
	public override void ClientRequestDisconnect (int outConnectionId){
		logMessage (String.Format("{0} left", playerInfoList[findWithConnID(outConnectionId)].name), "warning");
		base.ClientRequestDisconnect (outConnectionId);
	}
	public override void CouldNConnect (){
		base.CouldNConnect ();
		logMessage ("NoResponseReceived... Disconnected", "error");
	}
	public override void StopAllConnections (bool serverRequested){
		base.StopAllConnections (serverRequested);
		logMessage ("Stopping All Connections", "warning");
	}
	public override void receiveSyncOthers (ByteReceiver br){
		base.receiveSyncOthers (br);
		logMessage ("OthersReceived", "warning");
	}
	public override void sendServer (byte[] toSend, int channel){
		if(!client)logMessage ("Non client sending server", "error");
		base.sendServer (toSend, channel);
	}

	public void logMessage(string message){
		DebugConsole.Log (message);
	}
	public void logMessage(string message, Color color){
		DebugConsole.Log (message, color);
	}
	public void logMessage(string message, string type){
		DebugConsole.Log (message, type);
	}
}

