using UnityEngine;

public class ObjSpawningHelper{

	private Vector3[] availablePos;
	private Vector3[] posNormal;

	/*public ObjSpawning(){
		availablePos = new Vector3[1];
		posNormal = new Vector3[1];
		availablePos [0] = new Vector3 (0, 0, 0);
		posNormal [0] = Vector3.up;
	}*/
	/*public GameObject spawnObject (GameObject obj, Transform transToUse, int index, Vector3 offset){
		GameObject instance;
		instance = Object.Instantiate (obj);
		instance.transform.parent = transToUse;
		instance.transform.localPosition = availablePos[index];
		//Debug.Log (posNormal [index].ToString ());
		instance.transform.rotation = Quaternion.LookRotation(posNormal[index]);
		instance.transform.eulerAngles += offset;
		return instance;
	}*/
	public GameObject spawnObject (GameObject obj, Transform transToUse, int index, Vector3 rotation){
		GameObject instance;
		instance = Object.Instantiate (obj);
		instance.transform.parent = transToUse;
		instance.transform.localPosition = availablePos[index];
		instance.transform.eulerAngles = rotation;
		instance.transform.parent = null;
		return instance;
		//instance.transform.eulerAngles += rotOffset; 
	}
	public void updatePositions(Mesh mesh, Color[] colors, Color desColor){
		//int simp = colors.Length / mesh.vertices.Length;
		int count = (int)Mathf.Sqrt ((float)colors.Length);
		int meshCount = (int)Mathf.Sqrt ((float)mesh.vertices.Length);
		int simp = count / meshCount;
		int b = 0;

		for (int y = 0; y < count; y+=simp) {
			for (int x = 0; x < count; x+=simp) {
				if (colors [y * count + x].Equals (desColor))
					b++;
			}
		}
		
		availablePos = new Vector3[b];
		posNormal = new Vector3[b];
		b = 0;
		
		for (int y = 0; y < count; y+=simp) {
			for (int x = 0; x < count; x+=simp) {
				if (colors [y * count + x].Equals (desColor)) {
				
					int arrayIndex = (y / simp * meshCount) + (x / simp);

					availablePos [b] = mesh.vertices [arrayIndex];
					posNormal [b] = mesh.normals [arrayIndex];
					b++;
				}
			}
		}
		
		

	}

	public Vector3[] getAvalPos(){return availablePos;}
	public Vector3[] getNormals(){return posNormal;}

	/*public void updatePos(Mesh mesh, Color[] colors, Color[] desColors){
		mesh.RecalculateNormals ();

		int simp = colors.Length / mesh.vertices.Length;
		int b = 0;
		for (int ammount = 0; ammount < mesh.vertices.Length; ammount++)
			for (int i = 0; i < desColors.Length; i++)
				if (colors [ammount * simp].Equals (desColors [i])) 
					b++;

		availablePos = new Vector3[b];
		posNormal = new Vector3[b];
		b = 0;
		for (int ammount = 0; ammount < mesh.vertices.Length; ammount++)
			for (int i = 0; i < desColors.Length; i++)
				if (colors [ammount * simp].Equals (desColors [i])) {
					availablePos [b] = mesh.vertices [ammount];
					posNormal [b] = mesh.normals [ammount];
					b++;
				}

	}*/
}
