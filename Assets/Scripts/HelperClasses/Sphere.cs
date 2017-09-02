using UnityEngine;

public class Sphere{
	

	public static Vector3 getByDegrees(Vector2 sAngles){
		return getByDegrees (sAngles.x, sAngles.y);
	}
	public static Vector3 getByDegrees(float horizontalAngle, float verticalAngle){
		float x, y, z, radius;
		radius = Mathf.Sin (Mathf.Deg2Rad * verticalAngle);

		y = -Mathf.Cos (Mathf.Deg2Rad * verticalAngle);
		z = -Mathf.Cos (Mathf.Deg2Rad * horizontalAngle);
		x = Mathf.Sin (Mathf.Deg2Rad * horizontalAngle);

		z *= radius;
		x *= radius;

		return new Vector3 (x, y, z);
	}

	public static Vector2 getByPosition(Vector3 vector){
		vector.Normalize();
		float horizontalAngle, verticalAngle, radius;

		verticalAngle = Mathf.Acos (-vector.y) * Mathf.Rad2Deg;

		radius = Mathf.Sin (Mathf.Deg2Rad * verticalAngle);
		vector.x /= radius;
		horizontalAngle = Mathf.Asin (vector.x) * Mathf.Rad2Deg;

		return new Vector2 (horizontalAngle, verticalAngle);
	}

//	public static Vector3 approachSpherePos(Vector3 origin, Vector3 dest, float interp, Vector3 offset){
//		origin -= offset;dest -= offset;
//		return Vector3.Slerp (origin, dest, interp) + offset;
//	}

	public static Vector3 findVectorAndDistance(Vector2 a, Vector2 b){
		//float divisionFactor = Sphere.getDivisionFactor (a.y, b.y);
//		float xdiff, ydiff;
//		xdiff = a.x - b.x;
//		if (Mathf.Abs (xdiff) >= 180) {
//			if (xdiff >= 180)
//				xdiff -= 180;
//			else if (xdiff <= -180)
//				xdiff += 180;
//		}
//		ydiff = b.y - a.y;
		Vector2 vector = (b-a);

//		xdiff = Mathf.Abs (xdiff);
//		ydiff = Mathf.Abs (xdiff);
		float distance = vector.magnitude;//Mathf.Sqrt ((xdiff * xdiff) + (ydiff * ydiff));
		vector.Normalize();
		return new Vector3 (vector.x, vector.y, distance);
	}
		
	/*public static float getDivisionFactor(float A, float B){
		float C = (A + B) / 2;
		return Mathf.Sin ((C / 180) * Mathf.PI);
	}*/

	public static Vector3 getRotationToObject(Vector3 ownPos, Vector3 objectPos){
		Vector3 rotationVector = (objectPos - ownPos).normalized;
		Vector3 rotation = Quaternion.LookRotation(rotationVector).eulerAngles;
		return rotation;
	}
	public static Vector3 getRotationToObject(Vector3 ownPos, Vector3 objectPos, Vector3 upwards){
		Vector3 rotationVector = (objectPos - ownPos).normalized;
		Vector3 rotation = Quaternion.LookRotation(rotationVector, upwards).eulerAngles;
		return rotation;
	}

	public static Vector3[] createLine(Vector3 start, Vector3 end, int subdivisionNumber, float startHeight, float endHeight){
		subdivisionNumber += 2;
		Vector3[] vertsArray = new Vector3[subdivisionNumber];
		for (int i = 0; i < subdivisionNumber; i++) {
			float interpol = (float)i / (subdivisionNumber - 1);
			vertsArray [i] = Vector3.Slerp (start, end, (float)i / (subdivisionNumber - 1)) * ((endHeight * interpol) + (startHeight * ((float)1-interpol)));
		}
		return vertsArray;
	}

//	public static float getForce(float m1, float m2, float radius){
//		radius *= 0.1f;
//		return 0.0667f * m1 * m2 / (radius * radius);
//	}

	public static float areaCircle(float radius){
		return Mathf.PI * (radius * radius);
	}

	public static float circumCircle(float radius){
		return Mathf.PI * radius * 2;
	}

	public static float areaSphere(float radius){
		return 12.566370614f * (radius * radius);
	}

//	public static float volumeSphere(float radius){
//		return 4.188790205f/*which is (4/3 * PI)*/ * (radius * radius * radius);
//	}

	public static float getVolume(float radius){
		return 4.188790205f/*which is (4/3 * PI)*/ * (radius * radius * radius);
	}

	/*public static float findAngleByVector2(Vector2 vector){
		float angle = (Mathf.Acos (-vector.y) / Mathf.PI) * 180f;

		if (vector.x < 0)
			angle = (180-angle) + 180;
		return angle;
	}*/
}