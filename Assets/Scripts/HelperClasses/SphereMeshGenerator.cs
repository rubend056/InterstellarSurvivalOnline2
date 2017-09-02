using UnityEngine;
//using System.Collections;

public class SphereMeshGenerator {

	public static MeshData2 generateSphere(int horDivisions, int verDivisions,float radius){
		MeshData2 sphere = new MeshData2 (horDivisions,verDivisions);
		//int verticeAmmount = sphere.vertices.Length;

		float vAngleDiv = 180f / (horDivisions-1);
		float hAngleDiv = 360f / verDivisions;
		//Vertices
		int vertexCount = 0;
		for (int y = 0; y < horDivisions; y++) {
			for (int x = 0; x < verDivisions; x++) {
				sphere.vertices [vertexCount] = Sphere.getByDegrees (x * hAngleDiv, y * vAngleDiv);
				sphere.uvs [vertexCount] = new Vector2 ((float)x / (verDivisions-1f), (float)y / (horDivisions-1));
				//if (x == 0)Debug.Log()
				if (!(y == horDivisions - 1)) {
					int triA = vertexCount;
					int triB = vertexCount + 1;
					int triC = vertexCount + verDivisions;
					int triD = vertexCount + verDivisions + 1;
					if (x == verDivisions - 1) {
						triB = vertexCount - (verDivisions - 1);
						triD = vertexCount + 1;
					}
					sphere.AddTriangle (triA, triC, triB);
					sphere.AddTriangle (triC, triD, triB);
				}
				vertexCount++;
			}
		}

		//Triangles

		//Normals


		//UVs
		return sphere;
	}

//	public static Vector3 test(Vector3 h){
//		return Sphere.getByDegrees (h.x,h.y,h.z);
//	}

//	public static Vector3 getByDegrees(float hAngle, float vAngle, float height){
//		float x; float y; float z; bool check = false;
//		if (vAngle > 90)
//			vAngle -= 90;
//		else
//			check = true;
//		
//		x = Mathf.Sin (Mathf.Deg2Rad * vAngle) * height;
//		y = Mathf.Cos (Mathf.Deg2Rad * vAngle) * height;
//		if (!check) {
//			float j = x;
//			x = y;
//			y = j;
//		}
//		if (check)
//			y = -y;
//
//		float hx = hAngle;bool zinvert = true;
//		if (hx > 180) {
//			check = false;
//			if (hx > 270) {
//				hx -= 270;
//			}else {
//				hx -= 180;
//				zinvert = false;
//			}
//		} else {
//			check = true;
//			if (hx > 90) {
//				hx -= 90;
//				zinvert = false;
//			}
//		}
//		z = Mathf.Cos (Mathf.Deg2Rad * hx) * x;
//		hx = Mathf.Sin (Mathf.Deg2Rad * hx) * x;
//		if ((!zinvert && check) || (zinvert && !check)) {
//			float j = z;
//			z = hx;
//			hx = j;
//		}
//
//		if (!check)
//			hx = -hx;
//		
//		if (zinvert)
//			z = -z;
//		
//		return new Vector3 (hx, y, z);
//	}
}

public class MeshData2 {
	public Vector3[] vertices;
	public Vector3[] normals;
	public int[] triangles;
	public Vector2[] uvs;

	int triangleIndex;

	public MeshData2(int hSubdiv, int vSubdiv) {
		vertices = new Vector3[hSubdiv * vSubdiv];
		normals = new Vector3[hSubdiv * vSubdiv];
		uvs = new Vector2[hSubdiv * vSubdiv];
		triangles = new int[(hSubdiv * vSubdiv - vSubdiv)*6];
	}

	public void AddTriangle(int a, int b, int c) {
		triangles [triangleIndex] = a;
		triangles [triangleIndex + 1] = b;
		triangles [triangleIndex + 2] = c;
		triangleIndex += 3;
	}
	public void generateNormals(){
		for (int i = 0; i < vertices.Length; i++) {
			normals [i] = vertices [i].normalized;
		}
	}

	public Mesh CreateMesh() {
		generateNormals ();
		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();
		//mesh.normals = normals;
		return mesh;
	}

}
