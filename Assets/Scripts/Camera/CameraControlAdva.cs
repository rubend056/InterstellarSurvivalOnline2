using UnityEngine;
using System.Collections;
//using UnityEngine.Networking;

public class CameraControlAdva : MonoBehaviour {

	public static CameraControlAdva instance;
	public enum ViewMode{around, definitePos};

	public ViewMode viewMode = ViewMode.definitePos;
	public GameObject planet;

	public GameObject toFollow;
	//public GameObject disconnectButton;

	public float distance = 10f;
	[Range(-1f,1f)]
	public float yOffset = 0;
	public float lowestDist = 1f;
	public float highestDist = 60f;

	[Range(1f,50f)]
	public float smoothValue = 3f;


	[Range(0.5f,3f)]
	public float mouseAcceleration = 1f;

	public float planetFOV = 80;
	//public bool invert = false;

	//private GameObject spotlight;

	[HideInInspector]
	public PlayerController rotationToChange;

//	public GameObject playerObject;
	public GameObject pauseMenu;


	Vector2 goToSAngles = new Vector2 (0, 110);
	Vector2 lastToSAngles = new Vector2 (0, 110);
	public bool pause = false;
	public bool planetView = false;
	private GameObject previewObject;
	[HideInInspector]
	public Camera cam;
	private float normalFOV;
	//public VRInputModule vrInput;
	Transform myself;
	private float planetRadius = 1;
	private SmoothLookAtC lookAt;
	private Vector3 positionWanted = Vector3.zero;

//	private Vector3 localStartingPos;
//	private Transform startingParent;
	public bool touchEnabled = true;

	public bool targetOrientation = false;

	void Awake(){
		instance = this;
	}

	// Use this for initialization
	void Start () {
//		startingParent = gameObject.transform.parent;
//		localStartingPos = gameObject.transform.localPosition;
		//viewMode = ViewMode.around;
		lookAt = gameObject.GetComponent<SmoothLookAtC> ();
		cam = gameObject.GetComponent<Camera> ();
		normalFOV = cam.fieldOfView;
		if (toFollow && viewMode == ViewMode.around)
//			gameObject.transform.parent = toFollow.transform;
			changeFollow(toFollow);
		
		myself = gameObject.transform;
		//if (toFollowTransform != null)
			//myself.SetParent (toFollowTransform);
//		if (toFollow)
//			cursorCheck ();
	}

	//private Vector2 degreesOffset = Vector2.zero;
	// Update is called once per frame
	Vector2 lastDeltaChange = Vector2.zero, lastDelta = Vector2.zero;
	void Update () {

		/*if (spotlight){
			Ray ray = cam.ViewportPointToRay(new Vector3(0.5f,0.5f,0f));
			RaycastHit rayHit;
			if (Physics.Raycast(ray,out rayHit,50f)){
				spotlight.transform.rotation = Quaternion.Slerp(spotlight.transform.rotation, Quaternion.LookRotation ((rayHit.point - spotlight.transform.position).normalized),0.1f);
			}
		}*/
		/*if (vrInput && !Cursor.visible)
			vrInput.UpdateCursorPosition (new Vector2 (Screen.width/2, Screen.height/2));*/
		if (pauseMenu!=null)
			pauseMenuUpdate ();

		Vector3 beforePos = myself.position;

		if (viewMode == ViewMode.around) {
			if (toFollow) {
				
				if (!pause) {
					float mouseX = 0, mouseY = 0, scroll = 0;



					//Touch Control
					if (touchEnabled && Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Moved) {
						if (Input.touchCount == 1) {
							Vector2 touchDelta = Input.GetTouch (0).deltaPosition;
							mouseX = touchDelta.x;
							mouseY = touchDelta.y;

							lastDeltaChange = touchDelta - lastDelta;
							lastDelta = touchDelta;
						} else if (Input.touchCount >= 2) {
							Vector2 touchDelta0 = Input.GetTouch (0).deltaPosition;
							Vector2 touchDelta1 = Input.GetTouch (1).deltaPosition;
							scroll = (touchDelta0.y + touchDelta1.y) /2;
						}

						float divValue = 1 / (Screen.dpi * 0.1f);
						lastDeltaChange *= divValue;
						mouseX *= divValue;
						mouseY *= divValue;
						scroll /= Screen.dpi * 0.25f;
//						if (lastDeltaChange.magnitude > 5) {
//							mouseX = 0;
//							mouseY = 0;
//						}
					} else if (Input.GetMouseButton (2)) {//Right click
						//Mouse Control
						mouseX = Input.GetAxis ("Mouse X");
						mouseY = Input.GetAxis ("Mouse Y");
					}



					if (Input.mousePresent)scroll = Input.GetAxis ("Mouse ScrollWheel");

//					if (invert) {
//						mouseX = -mouseX;
//						mouseY = -mouseY;
//					}

					if (planetView) {
						mouseX = -mouseX;
						mouseY = -mouseY;
					}

					//Apply and clamp around angle :)						AroundAngle
					goToSAngles.x += mouseX * mouseAcceleration;
					if (goToSAngles.x < 0)
						goToSAngles.x += 360;
					if (goToSAngles.x > 360)
						goToSAngles.x -= 360;

					//Check and apply upangle, no clamping here :)			UpAngle
					float afterAround = goToSAngles.y + mouseY * mouseAcceleration;
					if ( 0 < afterAround && afterAround < 180)
						goToSAngles.y = afterAround;

					//Apply distance with smoothing, then clamp :) 			Distance
					distance = distance + distance * (-scroll * 0.5f);
					if (distance < lowestDist)
						distance = lowestDist;
					else if (distance > highestDist)
						distance = highestDist;
					
				}

				//Apply rotation and FOV with smoothing based on if it's a planet or not
				Vector3 toSet;
//				toSet = Vector3.Slerp (Sphere.getByDegrees (lastToSAngles), Sphere.getByDegrees (goToSAngles), 0.5f);
//				lastToSAngles = Sphere.getByPosition (toSet);
				toSet = Sphere.getByDegrees(goToSAngles);
				lastToSAngles = goToSAngles;

				toSet *= distance;
				if (planetView && planet != null) {
					cam.fieldOfView = (cam.fieldOfView * 0.95f) + (planetFOV * 0.05f);
				} else {
					toSet.y += yOffset * distance;

					cam.fieldOfView = (cam.fieldOfView * 0.95f) + (normalFOV * 0.05f);

					if (rotationToChange)
						rotationToChange.yOffset = -lastToSAngles.x;
				}
				if (targetOrientation)
					toSet = toFollow.transform.TransformDirection (toSet);
				positionWanted = toSet + toFollow.transform.position;
			}
		} else {
			cam.fieldOfView = (cam.fieldOfView * 0.95f) + (planetFOV * 0.05f);
//			myself.parent = startingParent;
//			myself.localPosition = localStartingPos;
			//if (Vector3.Distance (myself.localPosition,localStartingPos) < 0.00001f)
			//	disableYourself ();
		} 

		float smoothLocal = smoothValue;
		if (planetView)smoothLocal/=2;
		myself.position = Vector3.Lerp (beforePos, positionWanted, Time.unscaledDeltaTime * smoothLocal);

		//if (planet)
		//	togglePlanetView ();
	}

	private float lookYOffset;
	public void togglePlanetView(bool what){
			planetView = what;
			if (planetView) {
				//myself.parent = planet.transform;
				lookAt.target = planet.transform;

				lookYOffset = lookAt.YOffset;
				lookAt.YOffset = 0;
			} else {
				//myself.parent = toFollow.transform;
				lookAt.target = toFollow.transform;
				lookAt.YOffset = lookYOffset;
			}
		var pg3 = planet.GetComponent<PlanetGenerator3> ();
		if (pg3!=null)
			planetRadius = pg3.radius;
	}

	public void useTargetOrientation(bool value){
		if (toFollow == null)
			return;
		targetOrientation = value;
		if (lookAt != null)
			lookAt.useOtherOrient = value;


		Vector3 dir = positionWanted - toFollow.transform.position;
		if (value)
			dir = toFollow.transform.InverseTransformDirection (dir);
		Vector2 angles = Sphere.getByPosition (dir);
		lastToSAngles.x = angles.x;
		lastToSAngles.y = angles.y;
	}
	public void useTOToggle(){
		useTargetOrientation (!targetOrientation);
	}

	public void changeFollow(GameObject gO){
		toFollow = gO;
		lookAt.target = gO.transform;
		lookAt.otherOrientation = gO.transform;
		//if (viewMode == ViewMode.around)
		//	gameObject.transform.parent = gO.transform;

		if (gO.tag == "planet" || gO.tag == "planetRB" || gO.tag == "star") {
			planet = gO;
			//Basically a zoom into a planet by calculating it's up and around angle based on where it is
			// in reference to the planet and also taking the planet's radius into account :)

			togglePlanetView (true);
			float rad;
			if (gO.GetComponent<ConstraintTrans> () != null) {
				rad = gO.GetComponent<ConstraintTrans> ().otherTrans.
					gameObject.GetComponent<SphereCollider> ().radius;
			} else {
				rad = gO.GetComponent<SphereCollider> ().radius;
			}
			rad *= gO.transform.localScale.magnitude;

			lowestDist = rad * 0.8f;
			highestDist = rad * 7.5f;
			distance = rad * 1.6f;
		} else {
			float rad = 5;
			lowestDist = rad * 0.8f;
			highestDist = rad * 7.5f;
			distance = rad * 1.6f;
		}


		Vector3 dir = positionWanted - toFollow.transform.position;
		//dir = gO.transform.InverseTransformDirection (dir);
		Vector2 angles = Sphere.getByPosition (dir);
		lastToSAngles.x = angles.x;
		lastToSAngles.y = angles.y;

	}

	public void toggleViewType(ViewMode mode){
		DebugConsole.Log ("ViewTypeToggled:" + mode.ToString(), "normal");
		viewMode = mode;
//		if (viewMode == ViewMode.around) {
//			gameObject.transform.parent = toFollow.transform;
//		} else if (viewMode == ViewMode.definitePos) {
//			gameObject.transform.parent = null;
////			lookAt.target = planet.transform;
////			gameObject.transform.parent = startingParent;
//		}
		lookAt.viewMode = viewMode;
//		cursorCheck ();
	}



	void pauseMenuUpdate(){
		if (Input.GetKeyDown (KeyCode.Escape)) {
			togglePause (pauseMenu.activeSelf);
		}
	}

	public void togglePause(bool cm){
		pauseMenu.SetActive(!cm);
		//if (pauseMenu.activeSelf)disconnectButton.SetActive (CustomNMUI.instance.connected);
		toggleComponents (!cm);
	}
	public void togglePause(){
		bool cm = pauseMenu.activeSelf;
		pauseMenu.SetActive(!cm);
		//if (pauseMenu.activeSelf)disconnectButton.SetActive (CustomNMUI.instance.connected);
		toggleComponents (!cm);
	}

	void toggleComponents(bool cm){
//		if (playerObject!=null){
//			playerObject.GetComponent<PlayerController> ().enabled = cm;
//		}
			
		pause = cm;

		/*if (cm && CustomNMUI.instance.connected) {
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		} else {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}*/
	}

//	public void cursorCheck(){
//		if (toFollow.tag == "Player" && viewMode == ViewMode.around) {
//			Cursor.visible = false;
//			Cursor.lockState = CursorLockMode.Locked;
//		} else if (viewMode == ViewMode.definitePos){
//			Cursor.lockState = CursorLockMode.None;
//			Cursor.visible = true;
//		}
//	}

//	public void spawnPlayer(){
//		
//	}

	/*
	void changePreview(){
		if (objectToShow != actualOTS) {
			Debug.Log ("Changed");
			objectToShow = actualOTS;
			if (previewObject != null)
				GameObject.Destroy (previewObject);
			previewObject = GameObject.Instantiate (objectToShow);
			previewObject.transform.localScale = new Vector3(64,64,64);
			Component[] meshRArray;
			meshRArray = previewObject.GetComponentsInChildren<MeshRenderer>();
			previewObject.GetComponent<Animation> ().enabled = false;
			if (previewMat)
			foreach (MeshRenderer mr in meshRArray) {
				mr.sharedMaterial = previewMat;
			}
		}
	}

	void updatePreview(){
		if (toFollow.CompareTag ("Player") && (objectToShow)) {
			
			if (GetComponent (typeof(Camera)) != null) {
				Camera camera = GetComponent (typeof(Camera)) as Camera;

				//Raycast and find the position of other object********************

				RaycastHit hit;
				Ray ray = camera.ViewportPointToRay(new Vector3(0.5f,0.5f,0));
				if (Physics.Raycast (ray, out hit, 300f)) {
					previewObject.SetActive (true);
					Vector3 showingPoint = hit.point;
					Vector3 showingDirection = hit.normal;
					//showingDirection.y -= 90;

					previewObject.transform.position = showingPoint;
					previewObject.transform.rotation = Quaternion.LookRotation (showingDirection, toFollowTransform.up);
					Transform childTransform = previewObject.transform.GetChild (0);
					Vector3 localAngles = childTransform.localEulerAngles;
					localAngles.x -= 90;
					childTransform.localEulerAngles = localAngles;
				} else previewObject.SetActive(false);
			}
		}
	}*/

	void OnValidate(){
		useTargetOrientation (targetOrientation);
	}
}
