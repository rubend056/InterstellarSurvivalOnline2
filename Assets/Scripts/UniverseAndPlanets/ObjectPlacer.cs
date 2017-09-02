using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPlacer : MonoBehaviour {

	public float treeScale = 1;

	public float startSize = 0;
	public float finalSize = 1;
	public float finalSizeDev = 0.2f;
	public float growingSpeed = 0.1f;
	public int startSpawnNumber = 3;
	public bool doTrees = false;
	//public Color spawnColor;
	public Transform planetT;
	public bool areChildren = false;
	public RegionSpawn[] regionArray;
	[HideInInspector]
	public bool objectsShown = false;

	[Space(5)]
	[Header("PlayerPlacing")]
	public Color spawnColor;
	private SpawningInfo[] spawnPoints;

	Dictionary<int, GameObject> gameObjectDictionary;
	Dictionary<GameObject, int> IDDictionary;

	private Transform transToUse;
	private Color[] colors;
	private MeshFilter meshF;

	private ObjSpawningHelper[] os;

	private List<GameObject> spawnedGameObjects;
	public List<ObjectInfo> spawnedObjsInfo;

	public void Initialize(Color[] colorsLocal){
		colors = colorsLocal;

		meshF = gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter;
		transToUse = gameObject.transform;
		//making and updating AvailablePositions for the player, while transforming from local space to world space
		ObjSpawningHelper playerOS = new ObjSpawningHelper();
		playerOS.updatePositions (meshF.sharedMesh, colors, spawnColor);
		Vector3[] spawnPositions = playerOS.getAvalPos();
		Vector3[] spawnDirections = playerOS.getNormals ();
		//DebugConsole.Log ("numPositions:" + spawnPositions.Length.ToString ());
		spawnPoints = new SpawningInfo[spawnPositions.Length];
		for (int i = 0; i < spawnPoints.Length; i++) {
			spawnPoints [i].trans = transToUse;
			spawnPoints [i].position = spawnPositions [i];
			spawnPoints [i].rotation = spawnDirections [i];
		}
	}

	public void calculateObjectID(){
		gameObjectDictionary = new Dictionary<int, GameObject> ();
		IDDictionary = new Dictionary<GameObject, int> ();
		//Populate the dictionary with the keys for each object
		int count = 0;
		for (int i = 0; i < regionArray.Length; i++) {
			for (int e = 0; e < regionArray[i].objectArray.Length; e++){
				GameObject gO = regionArray [i].objectArray [e].obj;
				gameObjectDictionary.Add (count, gO);
				IDDictionary.Add (gO, count);
				count++;
			}
		}
	}

	public void spawnObjects(bool up = false){
		meshF = gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter;
		transToUse = gameObject.transform;
		spawnedObjsInfo = new List<ObjectInfo> ();
		spawnedGameObjects = new List<GameObject> ();

		if (doTrees) {
			os = new ObjSpawningHelper[regionArray.Length];

			for (int i = 0; i < os.Length; i++) {
				os [i] = new ObjSpawningHelper ();
				os [i].updatePositions (meshF.mesh, colors, regionArray [i].specificColor);
			}
			//Debug.Log ("Lenght: " + colors.Length.ToString() + "   " + colors [1].ToString ());
			for (int e = 0; e < os.Length; e++) {
				Vector3[] possiblePos = os [e].getAvalPos ();
				for (int i = 0; i < possiblePos.Length; i++) {

					int random = UnityEngine.Random.Range (0, 100);
					for (int r = 0; r < regionArray [e].objectArray.Length; r++) {
						if (regionArray [e].objectArray [r].percent >= random) {
							Vector3 rotationVar = regionArray [e].objectArray [r].rotOffVar * UnityEngine.Random.Range (-1f, 1f); 					//Getting randomized rotation
							Quaternion rotation = Quaternion.LookRotation (transToUse.TransformPoint (possiblePos [i]) - planetT.transform.position);
							if (up)
								rotation = Quaternion.Euler(-90,0,0);
							GameObject spawnedInstance = os [e].spawnObject (regionArray [e].objectArray [r].obj, transToUse, i, (rotation.eulerAngles + regionArray [e].objectArray [r].rotOffset + rotationVar));
							if (areChildren)
								spawnedInstance.transform.parent = gameObject.transform;

							spawnedInstance.transform.localScale = new Vector3 (treeScale, treeScale, treeScale);
							spawnedInstance.SetActive (false);
							spawnedGameObjects.Add (spawnedInstance);
							//Populating the info for networking
							ObjectInfo objInfo = new ObjectInfo ();
							objInfo.objType = IDDictionary [regionArray [e].objectArray [r].obj];
							objInfo.position = spawnedInstance.transform.position;
							objInfo.rotation = spawnedInstance.transform.rotation;
							spawnedObjsInfo.Add (objInfo);
							break;
						}
					}
				}
			}
		}
	}

	public void SyncAllTrees(ObjectInfo[] staffToSpawn){
		if (spawnedGameObjects!=null && spawnedGameObjects.Count > 0) {
			foreach (GameObject gO in spawnedGameObjects) {
				GameObject.Destroy (gO);
			}
			spawnedGameObjects.Clear ();
		}

		spawnedObjsInfo = new List<ObjectInfo> ();
		spawnedObjsInfo.AddRange (staffToSpawn);
		spawnedGameObjects = new List<GameObject> ();
		foreach (ObjectInfo objectI in staffToSpawn) {
			GameObject instance = (GameObject)Instantiate (gameObjectDictionary [objectI.objType]);
			instance.transform.position = objectI.position;
			instance.transform.rotation = objectI.rotation;
			spawnedGameObjects.Add (instance);
		}
	}

	public void setAll(bool value){
		if (spawnedGameObjects!=null && spawnedGameObjects.Count > 0) {
			foreach (GameObject gO in spawnedGameObjects) {
				gO.SetActive (value);
			}
		}
	}


	public void copyTo(ObjectPlacer tp){
		tp.treeScale = treeScale;
		tp.startSize = startSize;
		tp.finalSize = finalSize;
		tp.finalSizeDev = finalSizeDev;
		tp.growingSpeed = growingSpeed;
		tp.startSpawnNumber = startSpawnNumber;
		tp.doTrees = doTrees;
		tp.spawnColor = spawnColor;
		tp.areChildren = areChildren;
		tp.regionArray = regionArray;
		tp.planetT = planetT;
	}

	public SpawningInfo[] getSpawningInfo(){
		return spawnPoints;
	}
}

[System.Serializable]
public struct ObjectInfo{
	public int objType;
	public Vector3 position;
	public Quaternion rotation;
}

[System.Serializable]
public struct RegionSpawn{
	public string name;
	public Color specificColor;
	public ObjectSpawn[] objectArray;
}

[System.Serializable]
public struct ObjectSpawn{
	public string name;
	[Range(0,100)]
	public int percent;
	public GameObject obj;
	public bool enemy;
	public Vector3 rotOffset;
	public Vector3 rotOffVar;
}


public struct SpawningInfo{
	public Transform trans;
	public Vector3 position;
	public Vector3 rotation;
}