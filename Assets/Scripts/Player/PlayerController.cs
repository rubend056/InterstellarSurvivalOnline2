using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

	public static bool spawned = false;

//	public enum Continent{NorthAmerica, SouthAmerica, Europe, Africa, Antartica, Asia, Australia};
//
//	public Continent continent = Continent.NorthAmerica;

//	string playerName = "RubenD";
	private float speed;

	public float yOffset = 0;
	public float xOffset = 0;
	public float zOffset = 0;
	public float moveForce = 2000;
	public float jumpForce = 5000;
	public float animSpeedDiv = 2;
	public float ratioOut;
	public float maxSpeed = 5;
	public float initialBulletSpeed = 30f;
	public float bulletLifetime = 10f;
	public int maxBulletAmmount = 20;

	public int spawningListOffset = 0;

	//All other references **********
	private Transform cameraTransform;
	public GameObject playerPrefab;
	public GameObject bulletPrefab;
	public Transform planet;

	private SpawningManager spawnManager = null;

	[Space(5)]
	[Header("SpecificPlayerStaff")]
	public Animator anim;
	public Rigidbody ownRigidBody;
	public Transform ownTransform;
	public Transform torsoTransform;
	public Transform bulletSpawn;
	public Text nameText;
	public NetIdentityCustom ni;
	public GameObject healthBar;

	[Space(5)]
	[Header("AttachedClasses")]
	public PointerPick pointerPick;
	public static PlayerController instance;

	void Awake(){
		instance = this;
	}

	void Start(){
//		ni = gameObject.GetComponent<NetIdentityCustom> ();
//		if (ni.localPlayer) {
//			spawn ();
//		}
		spawningListOffset = NetTransportManager.instance.spawnableObjects.Count;
		NetTransportManager.instance.spawnableObjects.Add(playerPrefab);
	}

	// Update is called once per frame
	void Update () {
//		if (ni != null && !ni.localPlayer)
//			return;
		if (!spawned)
			return;
		inputControl ();
		alignToPlanet ();
//		fireHandler ();
	}

	public void changePlayerInstance(GameObject playerInst){
		var pii = playerInst.GetComponent<PlayerInstanceInfo> ();
		if (pii != null) {
			anim = pii.anim;
			ownRigidBody = pii.ownRigidBody;
			ownTransform = pii.ownTransform;
			torsoTransform = pii.torsoTransform;
			bulletSpawn = pii.bulletSpawn;
			nameText = pii.nameText;
			ni = pii.ni;
			healthBar = pii.healthBar;
			if (pointerPick != null) {
				pointerPick.gunTrans = pii.gunTrans;
			}
		}
		spawned = true;
	}

	public void spawn(){
		var planetInst = CameraControlAdva.instance.toFollow;
		var netInst = NetTransportManager.instance;
		var playerInst = netInst.spawnObject (
			                 spawningListOffset + 0,
			                 netInst.playerInfo.uniqueID,
			                 Vector3.zero,
			                 Quaternion.identity
		                 ).instance;
		changePlayerInstance (playerInst);
		UniverseManager.instance.planetUpdateTrans = playerInst.transform;
		UniverseCenter.instance.universeCenter = planetInst.transform;
		UniverseCenter.instance.switchCenter ();
		spawnPlayer (planetInst);
		playerInst.transform.parent = planetInst.transform;
	}

	private void spawnPlayer(GameObject whereToSpawn){

		//disabling HealthBar
		if (healthBar != null)
			healthBar.SetActive (false);

		//getting SpawnManager
		spawnManager = whereToSpawn.GetComponent<SpawningManager> ();
		//spawnManager.resetMPool (maxBulletAmmount, gameObject.GetComponent<NetworkIdentity>().connectionToServer);

		//spawning
		int randomNumber = (int)(Random.value * spawnManager.availableSpawns.Count);
		planet = whereToSpawn.transform;
		SpawningInfo spInfo = spawnManager.availableSpawns [randomNumber];
		ownTransform.position = spInfo.trans.TransformPoint (spInfo.position); 
		alignToPlanet ();
		ownTransform.position += (ownTransform.up * 50);


		//setting camera up
		GameObject cameraObj = GameObject.FindGameObjectWithTag ("MainCamera");
		cameraTransform = cameraObj.transform;

		var CCA = CameraControlAdva.instance;
		CCA.rotationToChange = this;
		CCA.planetView = false;
		CCA.changeFollow (ownTransform.gameObject);
		//CCA.invert = false;
		CCA.yOffset = 1.2f;
		CCA.toggleViewType (CameraControlAdva.ViewMode.around);
		CCA.targetOrientation = true;
		CCA.cursorCheck ();

		//CmdChangeContinent(CustomNMUI.instance.continent);
		SmoothLookAtC slac = cameraObj.GetComponent<SmoothLookAtC> ();
		slac.target = ownTransform;
		slac.useOtherOrient = true;



	}

	void inputControl(){
		

		float vertValue = Input.GetAxis ("Vertical");
		if (vertValue > 0) {
//			if (check) {
//				check = false;
//			}
			moveFoward (moveForce * ownRigidBody.mass);
		}
		RaycastHit rayHit;
		if (Physics.Raycast (ownTransform.position,-ownTransform.up, out rayHit, 0.8f)) {
			ownRigidBody.AddForce (Input.GetAxis ("Jump") * jumpForce * ownTransform.up);
		}

		speed = torsoTransform.InverseTransformDirection (ownRigidBody.velocity).z;
		speed /= animSpeedDiv;
		speed = Mathf.Clamp (speed, 0.5f, 2f);

		if (anim) {
			if (vertValue > 0) {
				anim.SetBool ("Foward", true);
				anim.SetFloat ("Speed", speed);
			}else
				anim.SetBool ("Foward", false);

			if (Input.GetAxis ("Sprint") > 0)
				anim.SetBool ("Running", true);
			else
				anim.SetBool ("Running", false);
		}
	}

	void moveFoward(float force){
		Vector3 pushDirection = torsoTransform.forward;
		float ratio = 0;

		Vector3 rayDir = Vector3.Lerp (pushDirection, -ownTransform.up, 0.75f);

		Ray ray = new Ray (ownTransform.position, rayDir);
		RaycastHit rayHit;
		if (Physics.Raycast (ray, out rayHit, 0.8f)) {
			ratio = (ownTransform.up - rayHit.normal).magnitude;
			ratioOut = ratio;
			ratio = Mathf.Clamp01 (ratio);
		}
		float maxLocalSpeed = maxSpeed;
		if (anim.GetBool ("Running"))
			maxLocalSpeed *= 1.7f;
		if (speed < maxLocalSpeed) {
			ownRigidBody.AddForce (pushDirection * (1 - ratio) * force * Time.deltaTime*60);
			ownRigidBody.AddForce (ownTransform.up * ratio * force * Time.deltaTime*60);
		}
	}

	void alignToPlanet(){
		Vector3 rotation = Quaternion.LookRotation((planet.position - ownTransform.position).normalized).eulerAngles;
		rotation.x -= 90;
		ownTransform.eulerAngles = rotation;
		Vector3 localAngles = new Vector3(0,yOffset,0);
		torsoTransform.localEulerAngles = localAngles;
	}

//	void fireHandler(){
//		if ( Input.GetMouseButtonUp(0) && !anim.GetBool ("Running")) {
//			CmdFire (bulletSpawn.position, bulletSpawn.forward);
//		}
//	}
//
//
//	void RpcPlaySound(float pitch){
//		AudioSource audio = bulletSpawn.GetComponent<AudioSource> ();
//		audio.pitch = pitch;
//		audio.Play ();
//	}
//
//
//	void CmdFire(Vector3 position, Vector3 rotVector){
//		// Set up bullet on server
//		var bullet = (GameObject)Instantiate(bulletPrefab, position, Quaternion.identity);
//		bullet.GetComponent<Rigidbody>().velocity = rotVector * initialBulletSpeed;
//
//		// spawn bullet on client, custom spawn handler will be called
////		NetworkServer.SpawnWithClientAuthority(bullet, connectionToClient);
//		AudioSource audio = bulletSpawn.GetComponent<AudioSource> ();
//		audio.pitch = Random.Range (0.8f, 1.8f);
//		audio.Play ();
//		RpcPlaySound (audio.pitch);
//
//		// when the bullet is destroyed on the server it wil automatically be destroyed on clients
//		StartCoroutine (Destroy (bullet, bulletLifetime));
//	}
//
//
//	public IEnumerator Destroy(GameObject gO, float timer)
//	{
//		yield return new WaitForSeconds (timer);
//		if (gO!=null)
//			CmdUnspawnObject (gO);
//	}
//
//	void OnCollisionEnter(Collision collision){
////		if (!ni.localPlayer)
////			return;
//		GameObject collisionObject = collision.gameObject;
//		if (collisionObject.tag == "Bullet" && !collisionObject.GetComponent<NetIdentityCustom>().HasAuthority) {
//			DebugConsole.Log ("tookDamage", "warning");
//			//gameObject.GetComponent<HealthController> ().CmdDamage (5f);
//
//			CmdUnspawnObject(collisionObject);
//		}
//	}
//
//
//	void CmdUnspawnObject(GameObject gO){
//		GameObject.Destroy (gO);
////		NetworkServer.UnSpawn (gO);
//	}
//
//
//	private void onPlayerChangeName(string name){
//		playerName = name;
//
//		if (NetTransportCustom.server) {
//			List<GameObject> players = new List<GameObject> ();
//			players.AddRange (GameObject.FindGameObjectsWithTag ("Player"));
//
//			if (players.Contains (this.gameObject))
//				players.Remove (this.gameObject);
//
//			bool foundPlayer = false;
//			for (int i = 0; i < players.Count; i++) {
//				if (players [i].GetComponent<PlayerController> ().playerName == name) {
//					//Communication.instance.TargetSendMessage (players [i].GetComponent<NetworkIdentity> ().connectionToClient, 
//					//	"A player has already joined with your username, your player will be disabled until then", Color.yellow, 2);
//					disableYourself (true);
//					foundPlayer = true;
//					break;
//				}
//			}
//			if (!foundPlayer)
//				disableYourself (false);
//		}
//
//		nameText.text = name;
//	}

////	[ServerCallback]
//	private void disableYourself(bool what){
//		RpcDisableYourself (!what);
//		this.gameObject.SetActive (!what);
//	}
//
////	[ClientRpc]
//	private void RpcDisableYourself(bool what){
//		this.gameObject.SetActive (what);
//	}
//
//	private IEnumerator waitToSetName(){
//		yield return new WaitForSeconds (2);
//		//setName(CustomNMUI.instance.playerName);
//	}
//
//	public void setName(string name){
//		playerName = name;
//		onPlayerChangeName (name);
//	}
//
////	[Command]
//	public void CmdChangeName(string name){
//		playerName = name;
//	}
//
////	[Command]
//	public void CmdChangeContinent(Continent continentLocal){
//		continent = continentLocal;
//	}
}
