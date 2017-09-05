using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class PointerPick : MonoBehaviour {

	public Inventory inv;
	public BuildInventory binv;
//	public PlayerController pc;
//	public CameraControlAdva cca;
//	public SmoothLookAtC slac;
	public GameObject toBuild;
	public GameObject buildUi;
	public float placeDistance = 10;
	public GameObject inventoryUi;
	//public GameObject activeIcon;
	public float orderDistance = 30;
	public GameObject pauseMenu;
	public Transform planet;

	//private PesantAI orderAI;

	private GameObject privateObj;
	private bool buildMode = false;
	private bool shown = false;
	public Transform gunTrans;

	private RaycastHit rayHit;
	private bool alreadyDone = false;
	private bool hitSomething = false;
	private bool isBuildObject = false;
	private NetIdentityCustom ni;
	private List<RaycastResult> results;
	// Use this for initialization

	private GameObject selectedPlanet;
	private GameObject previousPlanet;
	public static PointerPick instance;

	void Awake(){
		instance = this;
	}

	void Start () {
//		ni = gameObject.GetComponent<NetIdentityCustom> ();
//		if (!inv) {
//			inv = gameObject.GetComponent<Inventory> ();
//		}
//		if (!binv)binv = gameObject.GetComponent<BuildInventory> ();
		results = new List<RaycastResult> ();
//		if (ni.localPlayer) {
//			GameObject cameraObj = GameObject.FindGameObjectWithTag ("MainCamera");
//			cam = cameraObj.GetComponent<Camera> ();
//			cca = cameraObj.GetComponent<CameraControlAdva> ();
//			slac = cameraObj.GetComponent<SmoothLookAtC> ();
//			cca.playerObject = this.gameObject;
//
//			planet = GameObject.FindGameObjectWithTag ("planet").transform;
//
//			GameObject canvas = GameObject.Find("Canvas");
//			buildUi = canvas.transform.GetChild(2).gameObject;
//			inventoryUi = canvas.transform.GetChild(3).gameObject;
//			pauseMenu = canvas.transform.GetChild(4).gameObject;
//			//pauseMenu.transform.GetChild (3).GetChild (0).GetComponent<ButtonCustom> ().pp = this;
//		}
	}
	
	// Update is called once per frame
	void Update () {

		doRaycast ();

		if (Input.GetMouseButtonDown(0) && !PlayerController.spawned && !EventSystem.current.IsPointerOverGameObject()) {

			if (hitSomething) {
//				DebugConsole.Log (rayHit.collider.gameObject.tag);
				string tagL = rayHit.collider.gameObject.tag;
				if (rayHit.collider.gameObject != selectedPlanet && (tagL == "planet" || tagL == "planetRB" || tagL == "star")) {
					
					selectedPlanet = rayHit.collider.gameObject;
					var destruction = selectedPlanet.GetComponent<PlanetDestruction> ();
					if (destruction != null)
						selectedPlanet = destruction.linkedObject;
					
					previousPlanet = CameraControlAdva.instance.toFollow;
					CameraControlAdva.instance.changeFollow (selectedPlanet);
					if (tagL != "star")
						NetTransportManager.instance.togglePlayerSpawner (!PlayerController.spawned);
					else NetTransportManager.instance.togglePlayerSpawner (false);
				}

			}else {
				selectedPlanet = null;
				if (previousPlanet != null)
					CameraControlAdva.instance.changeFollow (previousPlanet);
				NetTransportManager.instance.togglePlayerSpawner (false);
			}

		}

//		if (buildMode) {
//			if (toBuild != null) {
//				if (hitSomething)
//				if (rayHit.distance <= placeDistance) {
//					buildUpdate ();
//				} else if (shown)
//					killToBuild ();
//			} else
//				killToBuild ();
//		} else if (hitSomething && rayHit.distance <= orderDistance) {
//			orderPesant ();
//		}

		if (gunTrans!=null){
			if (hitSomething && rayHit.distance <= orderDistance)
				gunTrans.rotation = Quaternion.Slerp (gunTrans.rotation, Quaternion.LookRotation ((rayHit.point - gunTrans.position).normalized), 0.5f);
			else
				gunTrans.rotation = Quaternion.Slerp (gunTrans.rotation, Quaternion.LookRotation (((CameraControlAdva.instance.cam.transform.position + (CameraControlAdva.instance.cam.transform.forward * 30)) - gunTrans.position).normalized), 0.5f);
		}

//		if (Input.GetMouseButtonUp (0)) {
//			if (buildMode) {
//				if (toBuild != null && shown)
//					placeObject ();
//			} else
//				selectPesant ();
//		}

//		if (Input.GetKeyDown (KeyCode.F)){
//			orderAI.work = !orderAI.work;
//		}
		alreadyDone = false;
	}

	void buildToggle(){
		if (Input.GetKeyDown (KeyCode.E)) {
			buildMode = !buildMode;
			buildUi.SetActive (buildMode);
		}
	}

	void inventoryToggle(){
		int pressedID = 0;
		if (Input.GetKeyDown (KeyCode.I))
			pressedID = 1;
		if (Input.GetKeyDown (KeyCode.R))
			pressedID = 2;
		
		if (pressedID>0){
			if (!inventoryUi.activeSelf) {
				inventoryUi.SetActive (true);

				toggleComponents (false);
				InventoryItemsUi iiUI = inventoryUi.GetComponentInChildren<InventoryItemsUi> ();
				if (pressedID == 1)
					iiUI.buildShow = false;
				else if (pressedID == 2)
					iiUI.buildShow = true;
				iiUI.updateItems ();
			} else {
				toggleComponents (true);
				inventoryUi.SetActive (false);

			}
		}
	}

	void buildUpdate(){
		//doRaycast ();
		if (hitSomething) {
			if (shown) {
				Transform trans = privateObj.transform;
				trans.position = rayHit.point;
				trans.rotation = Quaternion.LookRotation (rayHit.normal);
				Vector3 rot = trans.eulerAngles;
				rot.x += 90;
				trans.eulerAngles = rot;
			} else {
				privateObj = GameObject.Instantiate (toBuild);
				if (isBuildObject)
					privateObj.GetComponent<BuildingController> ().enabled = false;
				
				privateObj.SetActive (true);
				if (privateObj.GetComponent<Collider> () != null)
					privateObj.GetComponent<Collider> ().enabled = false;
				Component[] colliders = privateObj.GetComponentsInChildren<Collider>();
				foreach (Collider col in colliders) {
					col.enabled = false;
				}
				shown = true;
			}
		}
	}

	void killToBuild(){
		if (privateObj != null) {
			GameObject.Destroy (privateObj);
			privateObj = null;
		}
		shown = false;
	}

	void placeObject(){
		
		if (isBuildObject) {
			GameObject objInst = Object.Instantiate (toBuild,privateObj.transform.position,privateObj.transform.rotation);
			BuildingController buildC;
			if ((buildC = objInst.GetComponent<BuildingController> ()) != null)
				buildC.planet = planet;
		} 
//		else if (inv.removeObject (toBuild)){
//			GameObject objInst = Object.Instantiate (toBuild,privateObj.transform.position,privateObj.transform.rotation);
//			/*if (objInst.GetComponent<Collider> () != null)
//				objInst.GetComponent<Collider> ().enabled = true;*/
//		}
			
		List<thing> things;
		if (isBuildObject)
			things = binv.blueprints;
		else
			things = inv.things;
		if (Inventory.checkForName (things, toBuild.tag) == -1) {
			buildMode = false;
			buildUi.SetActive (false);
			killToBuild ();
		}


	}

//	void selectPesant(){
//		//doRaycast ();
//		if(hitSomething)
//		if (rayHit.transform.tag == "Pesant") {
//			//inv.addObject (rayHit.transform.gameObject);
//			orderAI = rayHit.transform.gameObject.GetComponent<PesantAI>();
//		}
//	}

	void toggleComponents(bool cm){
		PlayerController.instance.enabled = cm;
		CameraControlAdva.instance.pause = !cm;
		//cca.enabled = cm;
		//slac.enabled = cm;
//		if (cm) {
//			Cursor.visible = false;
//			Cursor.lockState = CursorLockMode.Locked;
//		} else {
//			Cursor.lockState = CursorLockMode.None;
//			Cursor.visible = true;
//		}
	}

	public void setBuildObject(string name, int e){
		if (e == 1) {
			toBuild = inv.things [Inventory.checkForName (inv.things, name)].gO;
			isBuildObject = false;
		}else if (e == 2) {
			isBuildObject = true;
			toBuild = binv.blueprints [Inventory.checkForName (binv.blueprints, name)].gO;
		}
	}

//	void orderPesant(){
//		if (Input.GetMouseButtonUp (2)) {
//			
//			if (Input.GetKey (KeyCode.LeftShift)) {
//				BuildingController bc;
//				if (rayHit.collider.tag == "Building" || rayHit.collider.tag == "BuildingUnfinished") {
//
//					bc = rayHit.collider.GetComponent<BuildingController> ();
//				} else if (rayHit.collider.transform.parent != null && (rayHit.collider.transform.parent.gameObject.tag == "Building" ||
//					rayHit.collider.transform.parent.gameObject.tag == "BuildingUnfinished")){
//
//					bc = rayHit.collider.transform.parent.gameObject.GetComponent<BuildingController> ();
//				} else
//					bc = null;
//				orderAI.setDropOff (rayHit.point, bc);
//			} else if (Input.GetKey (KeyCode.LeftControl)) {
//				TreeController tc;
//				if (rayHit.collider.tag == "Tree")
//					tc = rayHit.collider.GetComponent<TreeController> ();
//				else
//					tc = null;
//				orderAI.setPickup (rayHit.point,tc);
//			} else {
//				orderAI.goToCommand (rayHit.point);
//			}
//		}
//	}
	
	void doRaycast(){
		if (!alreadyDone) {
			Vector3 position = Input.mousePosition;
			position.x /= CameraControlAdva.instance.cam.pixelRect.width;
			position.y /= CameraControlAdva.instance.cam.pixelRect.height;

			Ray ray = CameraControlAdva.instance.cam.ViewportPointToRay (position);
			if (Physics.Raycast (ray, out rayHit, float.PositiveInfinity)) {
				hitSomething = true;
				//Debug.Log (rayHit.transform.gameObject.name);
			}else
				hitSomething = false;
			alreadyDone = true;

		}
	}


	void pauseMenuUpdate(){
		if (Input.GetKeyDown (KeyCode.Escape)) {
			togglePause ();
		}
	}
	public void togglePause(){
		pauseMenu.SetActive(!pauseMenu.activeSelf);
		toggleComponents (!pauseMenu.activeSelf);
	}





	private bool buildingUIShown = false;
	private GameObject activeUIGO;
	int uiWaitFrames = 0;
	void buildingUIUpdate(){
		
		if (hitSomething && rayHit.distance < placeDistance){ 
			//do something

		} else if (buildingUIShown) {
			if (RaycastWorldUI ())
				uiWaitFrames = 0;
			else {
				uiWaitFrames++;
				if (uiWaitFrames > (2/Time.smoothDeltaTime)) {
					buildingUIShown = false;
					activeUIGO.SetActive(false);
					uiWaitFrames = 0;
				}
			}
		}
	}

	bool RaycastWorldUI(){
		
		PointerEventData pointerData = new PointerEventData (EventSystem.current);
//		pointerData.position = new Vector2 (Input.mousePosition.x, Input.mousePosition.y);

		EventSystem.current.RaycastAll (pointerData, results);

		if (results.Count > 0) {
			string hitTag = results [0].gameObject.tag;
//			for (int i = 0; i < results.Count; i++) {
//				//Debug.Log (results [i].gameObject.tag + i.ToString ());
//			}

			if (hitTag == "BuildingUI" || hitTag == "BuildingUII") {
				//Debug.Log (hitTag);
				if (hitTag == "BuildingUII")
					results [0].gameObject.SendMessage ("beingHit");
				return true;
			} else if (results.Count > 1) {
				string hitTag2 = results [1].gameObject.tag;
				if (hitTag2 == "BuildingUI" || hitTag2 == "BuildingUII") {
					if (hitTag2 == "BuildingUII")
						results [0].gameObject.SendMessage ("beingHit");
					return true;
				}
			}
		}
		return false;
	}
}