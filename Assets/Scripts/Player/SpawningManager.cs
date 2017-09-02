using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpawningManager : MonoBehaviour {
	public List<SpawningInfo> availableSpawns;

//	void Start(){
//		Time.timeScale = 1f;
//	}
	/*
	public int m_defaultAmmount = 5;
	public GameObject m_bulletPrefab;
	public GameObject[] m_Pool;

	public NetworkHash128 assetId { get; set; }

	public delegate GameObject SpawnDelegate(Vector3 position, NetworkHash128 assetId);
	public delegate void UnSpawnDelegate(GameObject spawned);
	NetworkConnection netConn;

	void Start()
	{
		assetId = m_bulletPrefab.GetComponent<NetworkIdentity> ().assetId;

		ClientScene.RegisterSpawnHandler(assetId, SpawnObject, UnSpawnObject);
		//NetworkServer.RegisterHandler(MsgType.AddPlayer, OnAddPlayerMessage);
	}

	public bool resetMPool(int ammount, NetworkConnection conn){
		if (isPoolActive (m_Pool))
			return false;
		
		destroyPool (m_Pool);
		m_Pool = new GameObject[ammount];
		for (int i = 0; i < ammount; ++i)
		{
			m_Pool[i] = (GameObject)Instantiate(m_bulletPrefab, Vector3.zero, Quaternion.identity);
			m_Pool[i].name = "Bullet_" + i;
			if (conn != null)
				netConn = conn;
			m_Pool[i].SetActive(false);
		}
		return true;
	}

	public GameObject GetFromPool(Vector3 position)
	{
		if (m_Pool.Length == 0)
			resetMPool (m_defaultAmmount,null);
		
		foreach (var obj in m_Pool)
		{
			if (!obj.activeInHierarchy)
			{
				DebugConsole.Log("Activating object " + obj.name + " at " + position , "normal");
				obj.transform.position = position;
				obj.SetActive (true);

				TrailRenderer tr = obj.GetComponent<TrailRenderer> ();
				if (tr != null)
					tr.Clear ();

				return obj;
			}
		}

		DebugConsole.Log ("NoBulletsLeft!", "warning");
		return null;
	}

	public GameObject SpawnObject(Vector3 position, NetworkHash128 assetId)
	{
		return (GameObject)Instantiate(m_bulletPrefab, position, Quaternion.identity);
	}

	public void UnSpawnObject(GameObject spawned)
	{
		if (!spawned.GetComponent<NetworkIdentity>().hasAuthority)
			GameObject.Destroy(spawned);
	}
	*/
	/*public GameObject playerPrefab;

	public NetworkHash128 assetId { get; set; }

	public delegate GameObject SpawnDelegate(Vector3 position, NetworkHash128 assetId);
	public delegate void UnSpawnDelegate(GameObject spawned);

	void Start(){
		assetId = playerPrefab.GetComponent<NetworkIdentity> ().assetId;
		ClientScene.RegisterSpawnHandler(assetId, SpawnPlayerObject, UnSpawnPlayerObject);
	}

	public GameObject SpawnPlayerObject(Vector3 position, NetworkHash128 assetId)
	{
		return ;
	}

	public void UnSpawnPlayerObject(GameObject spawned)
	{
		GameObject player = GameObject.FindGameObjectWithTag ("Player");

	}*/
//	void OnPlayerConnected(NetworkPlayer player){
//		DebugConsole.Log ("PlayerConnected: " + player.ipAddress.ToString (), "normal");
//	}
//
//	void OnPlayerDisconnected(NetworkPlayer player){
//		DebugConsole.Log ("PlayerDisconnected: " + player.ipAddress.ToString (), "normal");
//	}
//
//	void OnConnectedToServer(){
//		DebugConsole.Log ("ConnectedToServer", "normal");
//	}

	/*void destroyPool(GameObject[] pool){
		foreach (GameObject gO in pool)
			if (gO.activeSelf) {
				GameObject.Destroy (gO);
			}
	}

	bool isPoolActive(GameObject[] pool){
		bool active = false;
		foreach (GameObject gO in pool)
			if (gO.activeSelf) {
				active = true;
				break;
			}
		
		return active;
	}
	*/
}


// shared class, this can be used by the client and the server as well
/*
public class Player : NetworkBehaviour
{
	[SerializeField]
	private GameObject _cameraPrefab = null; // camera prefab

	public override void OnStartLocalPlayer() // this is our player
	{
		base.OnStartLocalPlayer();

		GameObject cameraObj = GameObject.Instantiate(_cameraPrefab); // add camera
		cameraObj.GetComponent<MyCameraScript>().SetTarget(transform); // setup camera

		// add input handler component OR (see Update)
	}

	private void Update()
	{
		if (!base.isLocalPlayer)
			return;

		// update your input here
	}
}
*/
// server side
/*
public class PlayerSpawner : MonoBehaviour
{
	[SerializeField]
	private GameObject _playerPrefab = null;

	private void SpawnPlayer(NetworkConnection conn) // spawn a new player for the desired connection
	{
		GameObject playerObj = GameObject.Instantiate(_playerPrefab); // instantiate on server side
		NetworkServer.AddPlayerForConnection(conn, playerObj, 0); // spawn on the clients and set owner
	}


// server side

private void Setup()
{
	// ...

	NetworkServer.RegisterHandler(MsgType.AddPlayer, OnClientAddPlayer);
	// ...


private void OnClientAddPlayer (NetworkMessage netMsg)
	{

			Debug.Log ("Spawning player...");
			SpawnPlayer (netMsg.conn); // the above function
	}
}
*/