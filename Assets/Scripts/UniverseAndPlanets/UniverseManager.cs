using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

	public class UniverseManager : MonoBehaviour
{
	public const float massMultiplier = 0.1f;
//	public GameObject playerPrefab;
	public GameObject planetCPrefab;
	public GameObject[] planetPrefabs;
	public float maxSolarSystemExtent = 50000;
	public float minSolarSystemExtent = 1000;
	[Range(1,30)]
	public int maxNumPlanets = 0;
	private float sunRadius = 2800;
	public float initialSpeed = 100;
	[Range (1f, 90f)]
	public float gameSpeed = 1;
	public Transform player;
	public Text timeText;
	public static UniverseManager instance;


	public List<StarSystem> ssInstances;

	private int prefabOffset = 0;

	void Awake (){
		instance = this;
	}

	void Start (){
		Random.InitState ((int)System.DateTime.Now.Ticks);

		//Add the planetPrefabs to list of spawnable objects
		prefabOffset = NetTransportObjectSync.instance.spawnableObjects.Count;
		NetTransportObjectSync.instance.spawnableObjects.AddRange (planetPrefabs);

		//Initialize the StarSystemInstances with a new List
		ssInstances = new List<StarSystem> ();

		timeText.text = "";

		sunRadius = (planetPrefabs[0].transform.localScale.magnitude / 3) / 2;
		//CreatePlanets ();
		//StartCoroutine (CenterTest ());
	}

	public void CreatePlanets ()
	{
		//Create the first solar system*************************************
		ssInstances.Add (new StarSystem ());

		//Create the desired number of empty planet vessels*****************
		var planetInstaces = ssInstances [0].planets;
		planetInstaces.AddRange (new PlanetContainer[maxNumPlanets + 1]);

		//Initializing the empty planetVessels******************************
		for (int i = 0; i < planetInstaces.Count; i++)
			planetInstaces [i] = new PlanetContainer ();

		#region SunCreation
		//Creating the sun**************************************************
		var iatSun = NetTransportManager.instance.spawnObject(prefabOffset, NetTransportManager.instance.playerInfo.uniqueID, Vector3.zero, Quaternion.identity);
		planetInstaces [0].planet = iatSun.instance;	//Sun creation and save

		//Modify the planet information for the newly created sun ****************
//		var planetInformation0 = planetInstaces [0].planet.GetComponent<PlanetInformation> ();
//		planetInformation0.planetIndex = 0;
//		planetInformation0.starSystemIndex = 0;

		//Modify the planet information for the newly created sun ****************
		float sunMass = Sphere.getVolume (sunRadius) * massMultiplier;
		planetInstaces [0].planet.GetComponent<Rigidbody> ().mass = sunMass;

		//Update the network identity Variables******************
//		var ident = planetInstaces [0].planet.GetComponent<NetIdentityCustom> ();
		//ident.server = true;


		#endregion

		for (int i = 1; i < maxNumPlanets + 1; i++) {

			#region Planet Creation
			int prefabRandom = Random.Range (1, planetPrefabs.Length - 1);
			float planetRadius = Random.Range (100f, 150f);
			//planetPrefabs [planetRandom].GetComponent<PlanetGenerator3> ().radius = planetRadius;

			float distance = /*(sunRadius + planetRadius + 1000)*/ Random.Range (minSolarSystemExtent + planetRadius + 100, maxSolarSystemExtent);
			var planetPos = Random.onUnitSphere/*Vector3.right*/ * distance;//(Random.Range(sunRadius+planetRadius,maxSolarSystemExtent-planetRadius));
			DebugConsole.Log ("Distance:" + distance.ToString ());

			var iatPlanet = NetTransportManager.instance.spawnObject(prefabOffset + prefabRandom, NetTransportManager.instance.playerInfo.uniqueID, planetPos, Quaternion.identity);
			
			//Intantiating the collision and planet gameObjects and setting all their preferences
			planetInstaces [i].planet = iatPlanet.instance;
			planetInstaces [i].planetRB = (GameObject)Instantiate (planetCPrefab, planetPos, Quaternion.identity);
			planetInstaces [i].planetRB.GetComponent<Rigidbody> ().mass = Sphere.getVolume (planetRadius) * massMultiplier;
			planetInstaces [i].planetRB.GetComponent<SphereCollider> ().radius = planetRadius;
			
			planetInstaces [i].planet.GetComponent<NetTransformCustom> ().trans = planetInstaces [i].planetRB.transform;
			planetInstaces [i].planet.GetComponent<PlanetGenerator3> ().radius = planetRadius;
			planetInstaces [i].planet.GetComponent<PlanetGenerator3> ().player = player;
			planetInstaces [i].planetRB.GetComponent<PlanetDestruction> ().linkedObject = planetInstaces [i].planet;

			var constraint = planetInstaces [i].planet.GetComponent<ConstraintTrans> ();
			constraint.otherTrans = planetInstaces [i].planetRB.transform;
			constraint.pos = true;
			constraint.rot = true;
			
//			var planetInformation = planetInstaces [i].planet.GetComponent<PlanetInformation> ();
//			planetInformation.planetIndex = i;
//			planetInformation.starSystemIndex = 0;
			//Starting Orbit

			var lookRotation = planetInstaces [0].planet.transform.position - planetInstaces [i].planetRB.transform.position;
			planetInstaces [i].planetRB.transform.rotation = Quaternion.LookRotation (lookRotation.normalized);

			float rotationDir = Random.Range (0f, 1f);
			if (rotationDir < 0.5f)
				rotationDir = -1;
			else
				rotationDir = 1;// Generating rotation direction 1, or -1
			planetInstaces [i].planetRB.GetComponent<Rigidbody> ().AddTorque (Vector3.Lerp (planetInstaces [i].planetRB.transform.up, planetInstaces [i].planetRB.transform.right
																				, Random.Range (0.5f, 1f)).normalized * rotationDir * 5000, ForceMode.Impulse);
			//float initialSpeed = distance * (-12 / 2500) + 93.48f;

			float mass = planetInstaces [i].planetRB.GetComponent<Rigidbody> ().mass;
			float radius = lookRotation.sqrMagnitude;
			float gravity = Gravity.getGravityForce (mass, sunMass, radius);
			float force = Gravity.getOrbitSpeedByCentripetal (mass, gravity, Mathf.Sqrt(radius));

			//Apply velocity change to the planet
			planetInstaces [i].planetRB.GetComponent<Rigidbody> ().AddForce (planetInstaces [i].planetRB.transform.right * force, ForceMode.VelocityChange);
			DebugConsole.Log ("Force:" + force.ToString ());

			//Change the seed for the random generation
			planetInstaces [i].planet.GetComponent<PlanetGenerator3> ().seed = (int)System.DateTime.Now.Ticks;


//			var nID = planetInstaces [i].planet.GetComponent<NetIdentityCustom> ();
//
//			//Set all NetworkIdentity variables
//			nID.AuthorityID = NetTransportManager.instance.playerInfo.uniqueID;
//			nID.objID = NetTransportObjectSync.instance.generateID ();

			//Add planet to the NetObjectSync list of IDAndTransforms, then add it to this class' list
//			NetTransportObjectSync.instance.IDAndTransforms.Add (new IdentityAndTransform (planetInstaces [i].planet, 
//				NetObjBytes.ObjectType.Planet,
//				(prefabRandom + prefabOffset),
//				nID,
//				planetInstaces [i].planet.GetComponent<NetTransformCustom> ()));




			ssInstances [0].planets = planetInstaces;


			#endregion


//			if (i == 1) {
//				UniverseCenter.instance.universeCenter = planetInstaces [i].planetRB.transform;
//				UniverseCenter.instance.trenderers.Add (planetInstaces [i].planet.GetComponent<LineRenderer> ());
//				if (!UniverseCenter.instance.isCenter())
//					UniverseCenter.instance.switchCenter ();
//				planetInstaces [i].planet.GetComponent<PlanetGenerator3> ().collidersOn = true;
//			}
		}

		CameraControlAdva.instance.changeFollow(ssInstances[0].planets[0].planet);
	}

//	private List<GameObject> toSync;
	private Coroutine syncCoroutineValue;

	public void syncPlanet (IdentityAndTransform iat, float mass, Vector3 linVel, Vector3 angVel, int seed, float radius){
		//Reset coroutine (Coroutine controller)
//		if (toSync == null)
//			toSync = new List<GameObject> ();
//		toSync.Add (instance);
		if (syncCoroutineValue != null)
			StopCoroutine (syncCoroutineValue);
		syncCoroutineValue = StartCoroutine (syncCoroutine ());


		var pg3 = iat.instance.GetComponent<PlanetGenerator3> ();
		if (pg3 != null) {
			
			var rbInstance = (GameObject)Instantiate (planetCPrefab, iat.instance.transform.position, iat.instance.transform.rotation);
			var body = rbInstance.GetComponent<Rigidbody> ();
			rbInstance.GetComponent<SphereCollider> ().radius = radius;
			iat.instance.GetComponent<NetTransformCustom> ().trans = rbInstance.transform;
			rbInstance.GetComponent<PlanetDestruction>().linkedObject = iat.instance;
			iat.netTrans.trans = rbInstance.transform;
			
			var constraint = iat.instance.GetComponent<ConstraintTrans> ();
			constraint.otherTrans = rbInstance.transform;
			constraint.pos = true;
			constraint.rot = true;
			
			pg3.seed = seed;
			pg3.player = player;
			pg3.radius = radius;
			body.mass = mass;
			body.AddForce(linVel, ForceMode.VelocityChange);
			body.AddTorque(angVel, ForceMode.VelocityChange);
			if (ssInstances.Count == 0) {
				ssInstances.Add (new StarSystem ());
				ssInstances [0].planets = new List<PlanetContainer> ();
			}

			ssInstances [0].planets.Add (new PlanetContainer (iat.instance, rbInstance));
		} else {
			var body = iat.instance.GetComponent<Rigidbody> ();
			body.mass = mass;
			body.AddForce(linVel,ForceMode.VelocityChange);
			body.AddTorque(angVel, ForceMode.VelocityChange);
			if (ssInstances.Count == 0) {
				ssInstances.Add (new StarSystem ());
				ssInstances [0].planets = new List<PlanetContainer> ();
			}
			ssInstances [0].planets.Add (new PlanetContainer (iat.instance, null));
			iat.netTrans.trans = iat.instance.transform;
		}

		
	}

	private IEnumerator syncCoroutine (){
		yield return new WaitForSeconds(2);
		NetTransportObjectSync.instance.sendUniverseUpdateRequest(); //Values less than 0 mean universe update
		CameraControlAdva.instance.changeFollow(ssInstances[0].planets[0].planet);
	}

	private float sVal = 0;
	private int lVal = 0;

	void Update (){
		
		sVal += Time.deltaTime;
		if (sVal > lVal + 60) {
			lVal = (int)sVal;
			int minutes = (int)(sVal / 60);
			int hours = 0;
			while (minutes>=60){
				hours++;
				minutes-=60;
			}
			string toSet = "";
			if(hours>0)toSet += hours.ToString() + " hour(s) ";
			toSet += minutes.ToString () + " min";

			timeText.text = toSet;
		}
	}

	private bool findPlanetByObjID (int objID, out int starIndex, out int planetIndex){
	starIndex = 0;
	planetIndex = 0;
		for (int i = 0; i < ssInstances.Count; i++) {
			for (int e = 0; e < ssInstances [i].planets.Count; e++) {
				if (ssInstances [i].planets [e].planet.GetComponent<NetIdentityCustom> ().objID == objID){
					starIndex = i;
					planetIndex = e;
					return true;
				}
			}
		}
		return false;
	}

	public void removePlanet (int objID)
	{
		int starIndex, planetIndex;
		if (findPlanetByObjID (objID, out starIndex, out planetIndex)) {
			ssInstances [starIndex].planets.RemoveAt (planetIndex);
		}
	}

	public void OnValidate(){
		Time.timeScale = gameSpeed;
	}
}

[System.Serializable]
public class PlanetContainer
{
	public GameObject planet;
	public GameObject planetRB;

	public int ssIndex;
	public int planetIndex;
	public string planetName = "";


	public PlanetContainer ()
	{
	}

	public PlanetContainer (GameObject planetL, GameObject planetRBL)
	{
		planet = planetL;
		planetRB = planetRBL;
	}
}

[System.Serializable]
public class StarSystem
{
	public Vector3D systemPos;
	public List<PlanetContainer> planets;

	public StarSystem ()
	{
		planets = new List<PlanetContainer> ();
	}
}

public struct Vector3D
{
	public double x;
	public double y;
	public double z;

	public Vector3D (double xL, double yL, double zL)
	{
		x = xL;
		y = yL;
		z = zL;
	}

	public Vector3D (Vector3 vector)
	{
		x = System.Convert.ToDouble (vector.x);
		y = System.Convert.ToDouble (vector.y);
		z = System.Convert.ToDouble (vector.z);
	}

	public Vector3 toVector3 ()
	{
		return new Vector3 (
			System.Convert.ToSingle (x),
			System.Convert.ToSingle (y),
			System.Convert.ToSingle (z));
	}

	public double magnitude ()
	{
		return System.Math.Sqrt ((x * x) + (y * y) + (z * z));
	}
}