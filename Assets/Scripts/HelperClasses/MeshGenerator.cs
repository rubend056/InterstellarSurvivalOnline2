using UnityEngine;
using System.Collections;

public static class MeshGenerator {

	public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail, bool curve, float localHeight) {
		int width = heightMap.GetLength (0);
		int height = heightMap.GetLength (1);
		float topLeftX = (width - 1) / -2f;
		float topLeftZ = (height - 1) / 2f;

		levelOfDetail =  6-levelOfDetail;
		int meshSimplificationIncrement = (levelOfDetail <= 0)?1:levelOfDetail*2;
		int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

		float[,] modif = new float[height, width];
		if (curve) {
			float half = width / 2;
			for (int y = 0; y < height; y += meshSimplificationIncrement) {
				for (int x = 0; x < width; x += meshSimplificationIncrement) {

					modif [y, x] = -Mathf.Sqrt ((half) * (half) + (x - half) * (x - half)) + localHeight;

					float j = (-Mathf.Sqrt ((half) * (half) + (y - half) * (y - half)) + localHeight);
					if (j < modif [y, x])
						modif [y, x] = j;

					j = -Mathf.Sqrt ((half) * (half) + (y + x - half*2) * (y + x - half*2))/3 + localHeight*1/2;
					if (j < modif [y, x])
						modif [y, x] = j;

					j = -Mathf.Sqrt ((half) * (half) + (y - x) * (y - x ))/3 + localHeight*1/2;
					if (j < modif [y, x])
						modif [y, x] = j;
					
					for (int s = 1; s < 3; s++) {
					  float k = half * (s-1);
						j = -Mathf.Sqrt ((half) * (half) + (y*s - x - k) * (y*s - x - k))/(3*s) + localHeight*1/2;
						if (j < modif [y, x])
							modif [y, x] = j;
					}
				}
			}
		}

		MeshData meshData = new MeshData (verticesPerLine, verticesPerLine);
		int vertexIndex = 0;

		for (int y = 0; y < height; y+=meshSimplificationIncrement) {
			for (int x = 0; x < width; x+=meshSimplificationIncrement) {


				Vector3 toSet = new Vector3 (
					                topLeftX + x,
					                heightCurve.Evaluate (heightMap [x, y]) * heightMultiplier,
					                topLeftZ - y);
				if (curve){toSet.y += modif [y, x];
					if (x == 0 || y == 0 || y == height - 1 || x == width - 1)
						toSet.y = 0;
					/*else if (x < 4)
						toSet.y *= x / 3;
					else if (y < 4)
						toSet.y *= y / 3;
					else if (x > width - 4)
						toSet.y *= (y - (width - 4))/3;
					else if (y > height - 4)
						toSet.y *= (y - (height - 4))/3;*/
				}


				meshData.vertices [vertexIndex] = toSet;
				meshData.uvs [vertexIndex] = new Vector2 (x / (float)width, y / (float)height);

				if (x < width - 1 && y < height - 1) {
					meshData.AddTriangle (vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
					meshData.AddTriangle (vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
				}

				vertexIndex++;
			}
		}

		return meshData;

	}
}

public class MeshData {
	public Vector3[] vertices;
	public int[] triangles;
	public Vector2[] uvs;

	int triangleIndex;

	public MeshData(int meshWidth, int meshHeight) {
		vertices = new Vector3[meshWidth * meshHeight];
		uvs = new Vector2[meshWidth * meshHeight];
		triangles = new int[(meshWidth-1)*(meshHeight-1)*6];
	}

	public void AddTriangle(int a, int b, int c) {
		triangles [triangleIndex] = a;
		triangles [triangleIndex + 1] = b;
		triangles [triangleIndex + 2] = c;
		triangleIndex += 3;
	}

	public Mesh CreateMesh() {
		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateBounds ();
		mesh.RecalculateNormals ();
		return mesh;
	}

}