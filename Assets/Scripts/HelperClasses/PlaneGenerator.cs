using UnityEngine;
using System.Collections;

public static class PlaneGenerator {

	public static MeshData Generate(float width, float height, int widthSubdiv, int heightSubdiv){
		MeshData mesh = new MeshData(widthSubdiv+2, heightSubdiv+2);
		float widthAmmount = width / (widthSubdiv + 1);
		float heightAmmount = height / (heightSubdiv + 1);
		float xOrigin = -(width / 2);
		float yOrigin = -(height / 2);

		int vertexNumber = 0;
		for (int y = 0; y < heightSubdiv + 2; y++) {
			for (int x = 0; x < widthSubdiv + 2; x++) {
				//Positioning the Vertices
				mesh.vertices [vertexNumber] = new Vector3 (xOrigin + (x*(widthAmmount)), 0, yOrigin + (y*(heightAmmount)));

				//Adding Triangles
				if ((x != widthSubdiv + 1) && (y != heightSubdiv + 1)) {
					int A = vertexNumber;
					int B = vertexNumber + 1;
					int C = vertexNumber + widthSubdiv + 2;
					int D = vertexNumber + widthSubdiv + 3;
					mesh.AddTriangle (A, C, B);
					mesh.AddTriangle (C, D, B);
				}

				//UVs
				mesh.uvs[vertexNumber] = new Vector2((float)x/(widthSubdiv+1), (float)y/(heightSubdiv + 1));

				vertexNumber++;
			}
		}

		return mesh;
	}
}
