using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine.Networking;

public class NetTransportManager : MonoBehaviour{


	//NetworkIdentity something;

	//GeneralVariables
	public static int unreliableChannel;
	public static int reliableChannel;
	private bool connected = false;
	private int thisSocket = 0;
	private Coroutine connWaitCoroutine;

	//ServerVariables
	int serverSocket;
	public List<PlayerInfo> playerInfoList;
	private static int uniqueIDIndex = 1;

	//Clientvariables
	int clientSocket;
	public string ipAddress = "127.0.0.1";
	int clientConnectionID;

	//Additional Variables
	public PlayerInfo playerInfo;
	public static bool client = false;
	public static bool server = false;
	public int localPort = 2222;
	public int remotePort = 2222;

	public enum DataType {ConnAffirmation, Text, ListUpdate, Relay, RelayS, Sync};

	//For a global NetworkTransportManager instance
	public static NetTransportCustom instance;

	public virtual void Start(){
		
		Application.runInBackground = true;
		InitNetwork ();
		playerInfo = new PlayerInfo (true, "Me");
		playerInfoList = new List<PlayerInfo> ();
	}

	#region Update, StartServer and StartClient Functions
	//run every frame
	public virtual void Update(){
		if (client || server)
			updateNetPackages ();

		if (closeConnection) {
			if (client) {
				NetworkTransport.RemoveHost (clientSocket);
			} else if (server) {
				NetworkTransport.RemoveHost (serverSocket);
			}
			clientConnectionID = 0;
			playerInfoList.Clear ();
			server = false;
			client = false;
//			NetIdentityCustom.server = false;
//			NetIdentityCustom.client = false;
//			playerUniqueID = 0;
			uniqueIDIndex = 1;
			setConnected (false);
			closeConnection = false;
		}
	}
	public virtual void startAsServer(){
		//Check that neither one started
		if (server || client)
			return;

		//Setting up server
		server = true;
		SocketSetup ();

		setConnected (true);
	}
		
	public virtual void startAsClient(int offset = 1){
		//Check that neither one started
		if (server || client)
			return;
		
		//Settting up the Client
		client = true;
		SocketSetup (offset);
		Connect ();
	}
	#endregion

	private void setConnected(bool state){
		if(connected == !state)
			ConnStateChangeEvent (state);
		connected = state;
	}


	// Disabled the coroutine to enable updating per frame
//	private IEnumerator receiverCoroutine(){
//		while (client || server) {
//			yield return new WaitForSeconds (0.1f);
//			updateMessages ();
//		}
//	}

	void updateNetPackages(){
		int outHostId;
		int outConnectionId;
		int outChannelId;
		byte[] buffer = new byte[1024];
		int bufferSize = 1024;
		int receiveSize;
		byte error;

		NetworkEventType evnt = NetworkTransport.Receive(out outHostId, out outConnectionId, out outChannelId, buffer, bufferSize, out receiveSize, out error);
		while (evnt != NetworkEventType.Nothing) {

			if (((server && outHostId == serverSocket) || (client && outHostId == clientSocket))  && (NetworkError)error == NetworkError.Ok) {	//Just making sure packet came through our socket and no errors were thrown

				switch (evnt) {
				case NetworkEventType.ConnectEvent:

					if (client) {
						ServerRequestConnect (outConnectionId);
					} else {
						ClientRequestConnect (outConnectionId);
					}
					break;



				case NetworkEventType.DisconnectEvent:
					if (client && outConnectionId == clientConnectionID) {
						ServerRequestDisconnect ();
					} else if (findWithConnID (outConnectionId) != -1) {
						ClientRequestDisconnect (outConnectionId);
					}
					break;
			
				case NetworkEventType.DataEvent:
					ReceiveDataBasicEvent (outHostId, outConnectionId, outChannelId, buffer, receiveSize);
					break;
				}
			}
			evnt = NetworkTransport.Receive (out outHostId, out outConnectionId, out outChannelId, buffer, bufferSize, out receiveSize, out error);
		}
	}

	#region SendFunctions


	public void send(byte[] toSend, int connID,int channel){
		byte error;
		lowSendCommand (thisSocket, connID, reliableChannel, toSend, toSend.Length, out error);
		NetErrorEvent (error);
	}
	public void send(byte[] toSend, int[] connID, int channel){
		for (int i = 0; i < connID.Length; i++) {
			byte error;
			lowSendCommand (thisSocket, connID[i], reliableChannel, toSend, toSend.Length, out error);
			NetErrorEvent (error);
		}
	}
	public void send(byte[] toSend, int connID){
		send (toSend, connID, reliableChannel);
	}
	public void send(byte[] toSend, int[] connID){
		send (toSend, connID, reliableChannel);
	}

	public virtual void lowSendCommand(int socket, int connID, int channel, byte[] toSend, int length, out byte error){
		NetworkTransport.Send (socket, connID, channel, toSend, length, out error);
	}

	public virtual void sendServer(byte[] toSend, int channel){
		if (!client) 
			return;
		send (toSend, clientConnectionID, channel);
	}
	public void sendServer(byte[] toSend){
		sendServer (toSend, reliableChannel);
	}

	#endregion

	#region Connect/Disconnect

	public virtual void ClientRequestConnect(int outConnectionId){
		setConnected (true);

		ByteConstructor bc = new ByteConstructor();
		bc.add (System.BitConverter.GetBytes ((int)DataType.ConnAffirmation));
		send (bc.getBytes(), outConnectionId);
	}
	public virtual void ServerRequestConnect(int outConnectionId){
		setConnected (true);
	}

	public virtual void ServerRequestDisconnect(){
		StopAllConnections (true);
		//Something to do when server disconnected;
		playerInfoList.Clear ();
	}
	public virtual void ClientRequestDisconnect(int outConnectionId){
		int index = findWithConnID (outConnectionId);
		playerInfoList.RemoveAt (index);

//		if (playerInfoList.Count > 0)
//			setConnected (true);
//		else
//			setConnected (false);

		//		int uniqueIDL = findWithConnID (outConnectionId);
		//		updatePlayerList (NetRequestHandler.ListRequestType.Remove, uniqueIDL, "");
	}

	#endregion

	#region FindFunctions

	public int findWithConnID (int connID)
	{
		for (int i = 0; i < playerInfoList.Count; i++) {
			if (playerInfoList [i].connID == connID)
				return i;
		}
		return -1;
	}

	public int findWithUniqueID (int uniqueID)
	{
		for (int i = 0; i < playerInfoList.Count; i++) {
			if (playerInfoList [i].uniqueID == uniqueID)
				return i;
		}
		return -1;
	}

	public string getLocalIP()
	{
		IPHostEntry host;
		string localIP = "";
		host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (IPAddress ip in host.AddressList)
		{
			if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
			{
				localIP = ip.ToString();
				break;
			}
		}
		return localIP;
	}

	#endregion

	#region Setup/EndAll connecttions

	void SocketSetup (int offset = 1){

		// Create a connection_config and add a Channel.
		ConnectionConfig connection_config = new ConnectionConfig ();
		reliableChannel = connection_config.AddChannel (QosType.Reliable);
		unreliableChannel = connection_config.AddChannel (QosType.Unreliable);

		// Create a topology based on the connection config.
		HostTopology topology = new HostTopology (connection_config, 10);

		// Create a host based on the topology we just created, bound to a specific port
		if (client) {
			clientSocket = NetworkTransport.AddHost (topology, localPort + offset);
			thisSocket = clientSocket;
		} else {
			serverSocket = NetworkTransport.AddHost (topology, localPort);
			thisSocket = serverSocket;
		}
	}

	public virtual void InitNetwork (){
		GlobalConfig config = new GlobalConfig ();
		config.MaxPacketSize = 2048;
		NetworkTransport.Init (config);
	}

	public virtual void Connect (){
		byte error;
		clientConnectionID = NetworkTransport.Connect (clientSocket, ipAddress, remotePort, 0, out error);
		NetErrorEvent (error);
		connWaitCoroutine = StartCoroutine (connWaitFunc ());
	}

	private bool closeConnection = false;
	public virtual void StopAllConnections (bool serverRequested)
	{
		if (!serverRequested)
			Disconnect ();

		closeConnection = true;
	}

	private void Disconnect(){

		if (isConnected()) {
//			ByteContructor bc = new ByteContructor();
//			bc.add (System.BitConverter.GetBytes ((int)DataType.Disconnect));
//			if (isClient ())
//				send (bc.bytes.ToArray (), clientConnectionID);
//			else
//				foreach (PlayerInfo playerInfo in playerInfoList) {
//					send (bc.bytes.ToArray (), playerInfo.connID);
//				}
			byte error;
			if (isClient ()) {
				NetworkTransport.Disconnect (thisSocket, clientConnectionID, out error);
				NetErrorEvent (error);
			} else if (isServer ()) {
				foreach (PlayerInfo playerInfo in playerInfoList) {
					NetworkTransport.Disconnect (thisSocket, playerInfo.connID, out error);
					NetErrorEvent (error);
				}
			}
		}
	}

	#endregion

	#region Events

	public virtual void ConnStateChangeEvent(bool state){

	}
	public virtual void ReceiveDataEvent(DataType type, PlayerInfo playerInfo, byte[] data, int index = 0){
		switch (type) {
		case DataType.ConnAffirmation: 
			ReceiveConnAffirmation ();
			break;
		}
	}
	public void ReceiveDataBasicEvent (int hostId, int connectionID, int channelId, byte[] data, int size){
		ByteReceiver br = new ByteReceiver (data);
		var type = (DataType)br.getInt ();
		br.clean();
		ReceiveDataEvent (type, playerInfoList [findWithConnID (connectionID)], br.data);
	}
	public virtual void ReceiveConnAffirmation(){
		StopCoroutine (connWaitCoroutine);
	}
	public virtual void NetErrorEvent(byte error){
		
	}
	public virtual void CouldNConnect(){
		StopAllConnections (true);
	}


	#endregion

	public bool isConnected(){
		return connected;
	}

	public bool isClient(){
		if (!connected)
			return false;
		else
			return client;
	}
	public bool isServer(){
		if (!connected)
			return false;
		else
			return server;
	}

	public int generateUniqueID(){
		return uniqueIDIndex++;
	}

	void OnApplicationQuit(){
		StopAllConnections (false);
		NetworkTransport.Shutdown ();
	}

	IEnumerator connWaitFunc(){
		yield return new WaitForSeconds (1.5f);
		CouldNConnect ();
	}
}

