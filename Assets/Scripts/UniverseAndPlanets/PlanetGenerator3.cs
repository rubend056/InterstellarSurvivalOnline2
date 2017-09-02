using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;

public class PlanetGenerator3 : MonoBehaviour {

	//public static PlanetGenerator3 instance;

	[Range(0,10)]
	public int subCount = 0;
	int textQual = 240;

	[Range(1,100)]
	public int LOD = 6;
	[Range(1,100)]
	public int awayLOD = 12;
	public float radius = 300;
	public float density = 1;

	//Noise
	public float noiseScale = 300;
	public int octaves = 5;
	[Range(0,1)]
	public float persistance = 0.5f;
	public float lacunarity = 2;

	[Range(0f,2000f)]
	public float heightMuliplier = 1000;
	public AnimationCurve meshHeightCurve;
	//public bool useFalloff = true;
	public int seed = 0;


	public LODLevel[] lodLevel;
	public PhysicMaterial pMaterial;
	//public Vector2 offset;
	public GameObject waterPrefab;
	public float waterHeight = 0.1f;

	public bool planet = true;
	private bool sphere;
	public bool useNoise = true;
	public bool useFalloff = true;
	public bool autoUpdate = false;
	[HideInInspector]
	public bool hasTrees = false;

	[Space(5)]
	[Header("ObjectPlacer")]
	public ObjectPlacer objPlacerObj;
//	public bool placeOnStart = false;

	[Space(5)]
	[Header("PlanetUpdate")]
	//public PlayerPlacer playerPlacerObj;
	public Transform player;
	private float playerDistance;
	public bool dynamicUpdate = true;
	//public float opLODDistance = 100;
	//public bool opDynamicUpdate = true;
	//public float opDistance = 
	//public int placingTime = 5;
	//public int placingIndex = 9;

	[Space(3)]
	private float meshHeightMultiplier;
	public TerrainType[] regions;

	private GameObject[] terrainChunks;
	private float magicalOffset = 239;
	float[,] falloffMap;
	public PlanetInfo planetInfo;

	private float interval2;
	public delegate float[] calculateDistanceCaller(float[][] positions, float[] playerPos);
	private TerrainUpdater distThread;
	private IAsyncResult result;
	calculateDistanceCaller threadCaller;
	public SpawningManager psh;
	[HideInInspector]
	public bool collidersOn = false;

	//void Awake(){
	//	instance = this;
	//}

	//public ObjectPlacer objTest;
	//private int awayLOD;
	void Start(){
		//seed = (int)System.DateTime.Now.Ticks;
		//awayLOD = LOD;
		falloffMap = FallOffGenerator.GenerateFalloffMap (textQual);
		if (player!=null)distThread = new TerrainUpdater ();
		//QualitySettings.shadowDistance = radius;
		for(int i = 0;i<lodLevel.Length;i++)
			lodLevel [i].distance *= radius;
		if (psh == null) 
			psh = gameObject.GetComponent<SpawningManager> ();

		GenerateMap (true);
//		if (placeOnStart)
//			createTrees ();
		
	}

	private bool outsideCheck = false;
	private Coroutine playerCheckC;
	int t = 0, r = 0;// bool placerCheck = false;
	private bool lastColliderVal = false;
	void Update(){
		
		if (planet) {
			t++;
			if (t > 30) {
				t = 0;
				if (dynamicUpdate) {
					if (playerDistance < 2 * radius) {
//						if (playerCheckC != null) {
//							StopCoroutine (playerCheckC);
//							playerCheckC = null;
//							outsideCheck = false;
//						}
						outsideCheck = false;
						checkThread ();
					} else {
						if (!outsideCheck) {
							setAllLOD (awayLOD, false);
							outsideCheck = true;
						}
						updatePlayerDistance ();
					}
				} 
//				else {
//					if (playerCheckC != null) {
//						StopCoroutine (playerCheckC);
//						playerCheckC = null;
//					}
//				}
			}
			r++;
			if (r > 3000) {
				r = 0;
				Resources.UnloadUnusedAssets ();
			}
		}

		if (collidersOn != lastColliderVal) {
			foreach (GameObject gO in planetInfo.chunkObject) {
				if (collidersOn) {
					MeshCollider mc = gO.AddComponent (typeof(MeshCollider)) as MeshCollider;
					mc.sharedMesh = gO.GetComponent<MeshFilter> ().mesh;
				} else {
					GameObject.Destroy (gO.GetComponent<MeshCollider> ());
				}
			}
		}
			
		lastColliderVal = collidersOn;
		//if (!placerCheck && (int)Time.time > placingTime) {
			//placePlayer (9);
			//if(pp != null)
				//placeTrees (pp.indexOfPlacing);
		//}
	}

	#region Generation
	public void GenerateMap(bool putTrees = false) {
		if (objPlacerObj!=null)objPlacerObj.treeScale = radius/200;
		if (planet)
			sphere = true;
		else
			sphere = false;
		meshHeightMultiplier = radius * heightMuliplier / 1000;

		int count = gameObject.transform.childCount;
		for (int i = 0; i<count;i++)
			GameObject.DestroyImmediate(gameObject.transform.GetChild (0).gameObject);

		if (planet) {
			
			planetInfo = new PlanetInfo (subCount, radius);

			if (gameObject.GetComponent<MeshFilter>()!=null)
				GameObject.DestroyImmediate(gameObject.GetComponent<MeshFilter> ());
			if (gameObject.GetComponent<MeshRenderer>()!=null)
				GameObject.DestroyImmediate(gameObject.GetComponent<MeshRenderer> ());
			
			interval2 = radius / planetInfo.squareSectionAmmount;
			createNewChildren (/*putTrees*/);
			if (player!=null && dynamicUpdate && putTrees)threadInitializer ();
		} else {
			float[,] noiseMap = Noise.GenerateNoiseMap (textQual, textQual, seed, noiseScale, octaves, persistance, lacunarity, new Vector2(0,0), Noise.NormalizeMode.Global);
			Color[] colourMap = generateColorAndFalloff (noiseMap, textQual, useFalloff, false);

			GameObject gO = gameObject;
			MeshFilter mf;
			//Add MeshRenderer and MeshFilter
			if (gameObject.GetComponent<MeshFilter> () != null)
				mf = gameObject.GetComponent<MeshFilter> ();
			else
				mf = gameObject.AddComponent <MeshFilter>();
			
			if (gameObject.GetComponent<MeshRenderer> () == null)
				gameObject.AddComponent <MeshRenderer>();

			gO.layer = gameObject.layer;
			//gO.tag = gameObject.tag;

			updateMesh (LOD, mf, noiseMap, radius);

			Texture2D texture = new Texture2D (textQual, textQual);
			texture.filterMode = FilterMode.Point;
			texture.wrapMode = TextureWrapMode.Clamp;
			texture.SetPixels (colourMap);
			texture.Apply ();

			if (objPlacerObj!=null) {
				ObjectPlacer tp1 = this.GetComponent<ObjectPlacer>();
				tp1.Initialize (colourMap);
				//objPlacerObj.copyTo (tp1);
				bool localTree = tp1.doTrees;
				if (!putTrees && !tp1.areChildren)
					objPlacerObj.doTrees = false;
				//planetInfo.objPlacer [i] = tp1;
				tp1.spawnObjects (true);
				objPlacerObj.doTrees = localTree;
				//if (psh != null) {
				//	psh.availableSpawns = new List<SpawningInfo> ();
				//	SpawningInfo[] availableChunkSpawns = tp1.getSpawningInfo ();
				//	psh.availableSpawns.AddRange (availableChunkSpawns);
				//}
			} 


			gO.GetComponent<Renderer> ().sharedMaterial = new Material (Shader.Find ("Diffuse"));
			gO.GetComponent<Renderer> ().sharedMaterial.mainTexture = texture;
		}
		Resources.UnloadUnusedAssets ();
	}

	void createNewChildren(/*bool putTrees*/){
		//float interval2 = radius / planetInfo.squareSectionAmmount;
		MeshRenderer mr;
		//Generate the noise for all of the terrain
		for (int i = 0; i < planetInfo.chunksAmmount; i++) {
			Vector3 xyz = planetInfo.calculateXYZ (i);
//			float xDisp = -radius + interval2 + (interval2 * 2 * xyz.x);
//			float yDisp = -radius + interval2 + (interval2 * 2 * xyz.y);

			planetInfo.nmArray[i] = Noise.GenerateNoiseMap (textQual, textQual, seed, noiseScale, octaves, persistance, lacunarity, getOffset(xyz), Noise.NormalizeMode.Global);

			GameObject gO = new GameObject ();
			planetInfo.chunkArray [i] = gO.transform;
			planetInfo.chunkObject [i] = gO;
			gO.transform.parent = gameObject.transform;
			gO.transform.localPosition = new Vector3 (0, 0, 0);
			planetInfo.mfChunkArray [i] = gO.AddComponent (typeof(MeshFilter)) as MeshFilter;
			//MeshCollider mc = gO.AddComponent (typeof(MeshCollider)) as MeshCollider;
			//mc.enabled = false;
			//mc.convex = true;
			//if(pMaterial)mc.sharedMaterial = pMaterial;
			planetInfo.lodIdentifier [i] = LOD;
			//planetInfo.chunkObject [i].AddComponent<PlanetChunkID> ().chunkID = i;
			gO.transform.localEulerAngles = getAngle ((int)xyz.z);
			planetInfo.mcArray[i] = gO.AddComponent <MeshCollider>();
			planetInfo.mcArray [i].enabled = false;
		}
		//Fixing the terrain to concadenate well with eachother
//		for (int i = 0; i < planetInfo.chunksAmmount; i++) {
//			Vector3 xyz = planetInfo.calculateXYZ (i);
//			float xDisp = -radius + interval2 + (interval2 * 2 * xyz.x);
//			float yDisp = -radius + interval2 + (interval2 * 2 * xyz.y);
//			//fixTerrain ();
//		}
		// Material and Color
		for (int i = 0; i < planetInfo.chunksAmmount; i++) {
			planetInfo.colorArray[i] = generateColorAndFalloff (planetInfo.nmArray[i], textQual, useFalloff, false);
			Texture2D texture = new Texture2D (textQual, textQual);
			texture.filterMode = FilterMode.Point;
			texture.wrapMode = TextureWrapMode.Clamp;
			texture.SetPixels (planetInfo.colorArray [i]);
			texture.anisoLevel = 9;
			texture.Apply ();
			mr = planetInfo.chunkObject[i].AddComponent (typeof(MeshRenderer)) as MeshRenderer;
			mr.sharedMaterial = new Material (Shader.Find("Diffuse"));
			mr.sharedMaterial.mainTexture = texture;
			planetInfo.chunkObject [i].layer = gameObject.layer;
			planetInfo.chunkObject [i].name = ( gameObject.name + "_" + i.ToString());
		}
		if (waterPrefab) {
			GameObject water = GameObject.Instantiate (waterPrefab);
			water.transform.parent = gameObject.transform;
			water.transform.localPosition = Vector3.zero;
			float scale = radius * waterHeight;
			water.transform.localScale = new Vector3 (scale, scale, scale);
		}
		//Create the mesh and show the terrain
		if (psh == null) {
			var localGO = gameObject.GetComponent<SpawningManager>();
			if (localGO != null)
				psh = localGO;
		}

		if (objPlacerObj != null) {
			for (int i = 0; i < planetInfo.chunksAmmount; i++) {
				ObjectPlacer tp1 = planetInfo.chunkObject [i].AddComponent (typeof(ObjectPlacer)) as ObjectPlacer;
				objPlacerObj.copyTo (tp1);

				tp1.calculateObjectID ();
			}
		}

		StartCoroutine (updateAllMeshesCoroutine());
		//This is where the making of the mesh and making of trees was done now it was moved to a 
		// coroutine for immediate seeing of the meshes and possibly a more responsive UI


		//DebugConsole.Log ("AvailableSpawnPositions: " + psh.availableSpawns.Count.ToString (), "normal");
	}
	#endregion

	#region TerrainUpdate
	private void spawnAllObjects(){
		//planetInfo.generalObjectsInfo = new ObjectInfo[planetInfo.chunksAmmount][];
		//doTreesAfter = false;

		if (objPlacerObj != null) {
		for (int i = 0; i < planetInfo.chunksAmmount; i++) {
				ObjectPlacer tp1 = planetInfo.objPlacer [i];
				tp1.spawnObjects ();
				planetInfo.generalObjectsInfo [i] = tp1.spawnedObjsInfo.ToArray ();
			}
		}
	}


//	private bool doTreesAfter = false;
//	public void createTrees(){
////		if (!NetworkInfo.instance.doesNotNetwork) {
////			if (!isServer)
////				return;
////		}
//		if (planetInfo != null) {
//			spawnAllObjects ();
//		} else
//			doTreesAfter = true;
//	}


	public void SyncTrees(ObjectInfo[] generalObjectsInfoLocal, int chunckID){
		DebugConsole.Log ("Syncing Trees: " + chunckID.ToString());

		planetInfo.generalObjectsInfo[chunckID] = generalObjectsInfoLocal;
		planetInfo.objPlacer [chunckID].SyncAllTrees (generalObjectsInfoLocal);
		//DebugConsole.Log()
		/*for (int i = 0; i < generalObjectsInfoLocal.Length; i++) {
			
		}*/
	}


	void checkThread(){
		if (result != null && result.IsCompleted) {
			//bool check = true;
			try {
				float[] arrayResult = threadCaller.EndInvoke(result);
				if (arrayResult != null){
					playerDistance = findDisToPlanet (arrayResult);
					updateTerrain (arrayResult);
				}
			} catch (InvalidOperationException ioe) {
				DebugConsole.Log ("InvalidOPExeption :" + ioe.ToString(), "error");
			}


			result = null;
			if (player!=null && dynamicUpdate)threadInitializer ();

		}
	}

	void threadInitializer(){
		threadCaller = new calculateDistanceCaller (distThread.calculateDistance);
		float[] playerPos = new float[3];
		float[][] chunksPos = new float[planetInfo.chunksAmmount][];
		for (int i = 0; i < 3; i++) {
			playerPos [i] = player.position [i];
			for (int e = 0; e < planetInfo.chunksAmmount; e++) {
				if (i == 0)
					chunksPos [e] = new float[3];
				chunksPos [e] [i] = planetInfo.chunkArray [e].position [i];
			}
		}

		result = threadCaller.BeginInvoke (chunksPos, playerPos, null, null);
	}

	void updateTerrain(float[] distances){
		for (int i = 0; i < planetInfo.chunksAmmount; i++) {
			float dist = distances [i];
			bool good = false;

			for (int e = 0; e < lodLevel.Length; e++) {
				if (dist < lodLevel [e].distance) {
					if (planetInfo.lodIdentifier [i] != lodLevel[e].lod)
						setLOD (i, lodLevel [e].lod, lodLevel [e].showTrees, lodLevel[e].collisions);
					good = true;
					break;
				} 
			}

			if (!good){
				if (planetInfo.lodIdentifier [i] != awayLOD) {
					setLOD (i, awayLOD, false, false);
				}
			}

		}
	}

	private void setLOD(int index,int lod, bool trees, bool collisions){
		if (!planetInfo.finishedCreating)
			return;
		if (planetInfo.objPlacer [index] != null) planetInfo.objPlacer [index].setAll (trees);
		Vector3 xyz = planetInfo.calculateXYZ (index);
		float xDisp = -radius + interval2 + (interval2 * 2 * xyz.x);
		float yDisp = -radius + interval2 + (interval2 * 2 * xyz.y);
		planetInfo.chunkArray [index].localPosition = Vector3.zero;
		updateMesh (index,(int)xyz.z, planetInfo.chunkObject[index],planetInfo.chunkArray[index], lod, planetInfo.mfChunkArray [index], xDisp, yDisp, interval2, true);
		planetInfo.lodIdentifier [index] = lod;
		planetInfo.mcArray [index].enabled = collisions;
	}

	private void setAllLOD(int lod, bool trees){

		if (!planetInfo.finishedCreating)
			return;
		
		for (int i = 0; i < planetInfo.chunksAmmount; i++) {
			if (planetInfo.lodIdentifier [i] != lod)
				setLOD (i, lod, trees, false);
			else if (objPlacerObj != null) planetInfo.objPlacer [i].setAll (trees);
		}
	}
	private float findDisToPlanet(float[] distancesSqrd){
		float distance = distancesSqrd [0];
		for (int i = 1; i < distancesSqrd.Length; i++) {
			if (distancesSqrd [i] < distance)
				distance = distancesSqrd [i];
		}
		return (Mathf.Sqrt(distance) + radius);
	}
	private void updatePlayerDistance(){
		playerDistance = Vector3.Distance (gameObject.transform.position, player.position);
	}
	#endregion

	#region MeshAndMaterials
	void updateMesh(int lod, MeshFilter mf, float[,] noiseMap, float interval2){
		if (mf.sharedMesh!=null)mf.sharedMesh.Clear();

		MeshData mesh = PlaneGenerator.Generate (interval2 * 2, interval2 * 2, textQual / lod - 2, textQual / lod - 2);
		mesh.vertices = addNormalNoise (mesh.vertices, noiseMap, meshHeightMultiplier, false);

		mf.sharedMesh = mesh.CreateMesh ();
	}

	void updateMesh(int index,int idz, GameObject terrainObject, Transform gO, int lod, MeshFilter mf, float xDisp, float yDisp, float interval2, bool update){
		//if (mf.sharedMesh!=null)mf.sharedMesh.Clear();

		MeshData meshData = PlaneGenerator.Generate (interval2 * 2, interval2 * 2, textQual / lod - 2, textQual / lod - 2);
		if (sphere)
			meshData.vertices = spheritize (meshData.vertices, new Vector3 (xDisp, radius, yDisp));
		if (useNoise)
			meshData.vertices = addNormalNoise (meshData.vertices, planetInfo.nmArray [index], meshHeightMultiplier);
		Vector3 movement = meshAverage (meshData.vertices);
		gO.localPosition += translateByPivot(idz,new Vector2(movement.x, movement.z), movement.y);
		meshData.vertices = translateMesh (meshData.vertices, -movement);
		//Debug.Log (mesh.vertices.Length.ToString());
		Mesh mesh = meshData.CreateMesh ();
		if (!update)
			mf.sharedMesh = mesh;
		else
			mf.mesh = mesh;

		planetInfo.mcArray [index].sharedMesh = mesh;
	}

	Vector2 getOffset(Vector3 xyz){
		Vector2 actualOffset;
		if(xyz.z<4)
			actualOffset = new Vector2 (((planetInfo.squareSectionAmmount) * magicalOffset)*xyz.z + (xyz.x * (magicalOffset)), xyz.y * magicalOffset);
		else if(xyz.z==4)
			actualOffset = new Vector2 (xyz.x * (magicalOffset), -(magicalOffset * planetInfo.squareSectionAmmount + magicalOffset*xyz.y));
		else 
			actualOffset = new Vector2 ((xyz.x * (magicalOffset)), (magicalOffset * (planetInfo.squareSectionAmmount - xyz.y)));

		//Debug.Log (xyz.ToString () + actualOffset.ToString ());
		return actualOffset;
	}

	void fixTerrain(int i){

	}

	Color[] generateColorAndFalloff(float[,] noiseMap, int count, bool useFalloff, bool flat){
		Color[] colourMap = new Color[count * count];
		for (int y = 0; y < count; y++) {
			for (int x = 0; x < count; x++) {

				if (!flat) {
					if (useFalloff) {
						noiseMap [x, y] = Mathf.Clamp01 (noiseMap [x, y] - falloffMap [x, y]);
					}
				} else
					noiseMap [x, y] = 0;

				float currentHeight = noiseMap [x, y];
				for (int i = 0; i < regions.Length; i++) 
					if (currentHeight >= regions [i].height)
						colourMap [y * count + x] = regions [i].colour;
					else break;
			}
		}
		return colourMap;
	}

	Vector3[] addNormalNoise(Vector3[] mesh, float[,] noise,float heightMultiplier, bool referenceBased){
		int max = (int)Mathf.Sqrt ((float)mesh.Length);
		int simp = (int)Mathf.Sqrt ((float)noise.Length) / max;
		int count = 0;
		for (int y = 0; y < max; y ++) {
			for (int x = 0; x < max; x ++) {
				if (referenceBased)
					mesh [count] = mesh [count] * ((meshHeightCurve.Evaluate (noise [x*simp, y*simp]) * (heightMultiplier / radius)) + 1);
				else
					mesh [count].y = heightMultiplier * meshHeightCurve.Evaluate (noise [x*simp, y*simp]);
				count++;
			}
		}
		return mesh;
	}

	Vector3[] addNormalNoise(Vector3[] vertices, float[,] noise, float heightMultiplier){
		int max = (int)Mathf.Sqrt ((float)vertices.Length);
		int simp = (int)Mathf.Sqrt ((float)noise.Length) / max;

		int count = 0;
		for (int y = 0; y < max; y ++) {
			for (int x = 0; x < max; x ++) {
				vertices [count] += vertices[count].normalized * heightMultiplier * meshHeightCurve.Evaluate (noise [x*simp, y*simp]);
				count++;
			}
		}
		return vertices;
	}

	Vector3[] spheritize(Vector3[] mesh, float rad){
		for (int i = 0; i < mesh.Length; i++) {
			mesh [i] = (mesh [i]).normalized * rad;
		}
		return mesh;
	}

	Vector3[] spheritize(Vector3[] vertices, Vector3 disp){
		for (int i = 0; i < vertices.Length; i++) {
			vertices [i] += disp;
			vertices [i] = (vertices [i]).normalized * disp.y;
		}
		return vertices;
	}

	Vector3 getAngle(int id){
		Vector3 rotation;
		float interval = 90;
		if (id == 0)rotation = new Vector3 (-interval, 0, 0);
		else if (id == 1)rotation = new Vector3 (-interval, 0, -interval);
		else if (id == 2)rotation = new Vector3 (-interval, 0, -interval*2);
		else if (id == 3)rotation = new Vector3 (-interval, 0, -interval*3);
		else if (id == 4)rotation = new Vector3 (0, 0, 0);
		else if (id == 5)rotation = new Vector3 (interval*2, 0, 0);
		else rotation = new Vector3 (0, 0, 0);
		return rotation;
	}

	Vector3 getPosition(int id, float interval, Vector3 xyz){
		float interval2 = interval / planetInfo.squareSectionAmmount;

		float xDisp = -interval + interval2 + (interval2 * 2 * xyz.x);
		float yDisp = -interval + interval2 + (interval2 * 2 * xyz.y);

		Vector3 position;
		if (xyz.z == 0)position = new Vector3 (xDisp, yDisp, -interval);
		else if (xyz.z == 1)position = new Vector3 (interval, yDisp, xDisp);
		else if (xyz.z == 2)position = new Vector3 (-xDisp, yDisp, interval);
		else if (xyz.z == 3)position = new Vector3 (-interval, yDisp, -xDisp);
		else if (xyz.z == 4)position = new Vector3 (xDisp, interval, yDisp);
		else if (xyz.z == 5)position = new Vector3 (xDisp, -interval, -yDisp);
		else position = new Vector3 (0, 0, 0);

		return position;
	}

	static Vector3 translateByPivot(int id, Vector2 translation, float radius){
		Vector3 position;
		if (id == 0)position = new Vector3 (translation.x, translation.y, -radius);
		else if (id == 1)position = new Vector3 (radius, translation.y, translation.x);
		else if (id == 2)position = new Vector3 (-translation.x, translation.y, radius);
		else if (id == 3)position = new Vector3 (-radius, translation.y, -translation.x);
		else if (id == 4)position = new Vector3 (translation.x, radius, translation.y);
		else if (id == 5)position = new Vector3 (translation.x, -radius, -translation.y);
		else position = new Vector3 (translation.x, radius, translation.y);

		return position;
	}

	Vector3 meshAverage(Vector3[] mesh){
		Vector3 average = Vector3.zero;
		int i;
		for (i = 0; i < mesh.Length; i++) {
			average.x += mesh [i].x;
			average.y += mesh [i].y;
			average.z += mesh [i].z;
		}
		average.x /= i + 1;
		average.y /= i + 1;
		average.z /= i + 1;
		return average;
	}

	Vector3[] translateMesh(Vector3[] vertices, Vector3 translation){
		for (int i = 0; i < vertices.Length; i++) {
			vertices [i] += translation;
		}
		return vertices;
	}
	#endregion

	private IEnumerator updateAllMeshesCoroutine(){
		for (int i = 0; i < planetInfo.chunksAmmount; i++) {
			Vector3 xyz = planetInfo.calculateXYZ (i);
			float xDisp = -radius + interval2 + (interval2 * 2 * xyz.x);
			float yDisp = -radius + interval2 + (interval2 * 2 * xyz.y);
			updateMesh (i,(int)xyz.z, planetInfo.chunkObject[i],planetInfo.chunkArray[i], LOD, planetInfo.mfChunkArray [i], xDisp, yDisp, interval2, false);

			if (objPlacerObj != null) {
				ObjectPlacer tp1 = planetInfo.chunkObject [i].AddComponent (typeof(ObjectPlacer)) as ObjectPlacer;
				objPlacerObj.copyTo (tp1);
				tp1.Initialize (planetInfo.colorArray [i]);
				tp1.calculateObjectID ();
				planetInfo.objPlacer [i] = tp1;
				if (psh != null) {
					if(psh.availableSpawns == null)
						psh.availableSpawns = new List<SpawningInfo> ();	
					psh.availableSpawns.AddRange (tp1.getSpawningInfo ());
				}
			}
			yield return null;
		}

		if (objPlacerObj != null) {
			spawnAllObjects ();
			hasTrees = true;
		}
		planetInfo.finishedCreating = true;
		setAllLOD (awayLOD, false);
	}

	void OnValidate() {
		if (lacunarity < 1) {
			lacunarity = 1;
		}
		if (octaves < 0) {
			octaves = 0;
		}

		if (density < 0)
			density = 0;

		if(useFalloff)falloffMap = FallOffGenerator.GenerateFalloffMap (textQual);
	}
}

[System.Serializable]
public struct LODLevel{
	public float distance;
	[Range(1,20)]
	public int lod;
	public bool showTrees;
	public bool collisions;
}

public class PlanetInfo{
	public bool finishedCreating = false;
	public float[][,] nmArray;
	public Color[][] colorArray;
	public ObjectInfo[][] generalObjectsInfo;
	public int[] lodIdentifier;
	public GameObject[] chunkObject;
	//public Vector3[] beforePos;
	//public Vector3[] localPos;
	public Transform[] chunkArray;
	public MeshFilter[] mfChunkArray;
	public ObjectPlacer[] objPlacer;
	public MeshCollider[] mcArray;
	public float chunkSize;
	public int chunksAmmount;
	public int sectionAmmount;
	public int squareSectionAmmount;


	public PlanetInfo(int subdiv, float radius){
		chunksAmmount = 6 * ((subdiv+1) * (subdiv+1));
		sectionAmmount = chunksAmmount / 6;
		squareSectionAmmount = subdiv + 1;
		chunkSize = radius/squareSectionAmmount;
		generalObjectsInfo = new ObjectInfo[chunksAmmount][];
		chunkObject = new GameObject[chunksAmmount];
		objPlacer = new ObjectPlacer[chunksAmmount];
		//beforePos = new Vector3[chunksAmmount];
		mcArray = new MeshCollider[chunksAmmount];
		lodIdentifier = new int[chunksAmmount];
		chunkArray = new Transform[chunksAmmount];
		mfChunkArray = new MeshFilter[chunksAmmount];
		nmArray = new float[chunksAmmount][,];
		colorArray = new Color[chunksAmmount][];
	}

	public Vector3 calculateXYZ(int index){
		int f = index;
		int z = 0;
		while (f > sectionAmmount-1) {
			f -= sectionAmmount;
			z++;
		}
		int x = f;
		int y = 0;
		while (x > squareSectionAmmount-1) {
			x -= squareSectionAmmount;
			y++;
		}
		return new Vector3 (x, y, z);
	}

	public int rebuildIndex(Vector3 xyz){
		int index = 0;
		index = (int)xyz.x + (int)xyz.y * squareSectionAmmount + (int)xyz.z * sectionAmmount;
		return index;
	}

	/*public ObjectPlacer getObjPlacerByPos2 (Vector2 pos)
	{
		
	}*/
}
public class TerrainUpdater{
	public float[] calculateDistance(float[][] positions, float[] playerPos){
		float[] distance = new float[positions.Length];
		for (int i = 0; i < positions.Length; i++) {
			float[] diff = new float[3];
			for (int e = 0; e < 3; e++)
				diff[e] = positions [i][e] - playerPos[e];
			distance [i] = (float)System.Math.Sqrt ((diff [0] * diff [0]) + (diff [1] * diff [1]) + (diff [2] * diff [2]));
		}
		return distance;
	}
}

[System.Serializable]
public struct TerrainType {
	public string name;
	public float height;
	public Color colour;
}