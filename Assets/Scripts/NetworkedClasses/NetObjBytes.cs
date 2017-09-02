using UnityEngine;
using System;
using System.Collections.Generic;

public class NetObjBytes {
	public enum ObjectType{Generic, Rigidbody, Planet};

//	public static byte[] genericSpawn(int objectID, int authorityID,int prefabIndex, Vector3 pos, Vector3 rot){
//		ByteConstructor bc = new ByteConstructor();
//		bc.add(NetObjSpawn.defaultInfo((int)NetObjSpawn.ObjectType.Generic,objectID,authorityID,prefabIndex,pos, rot));
//		return bc.getBytes ();
//	}

	#region Specific Objects
	public static byte[] defaultInfo(int objectID, int authorityID, int type, int prefabIndex, Vector3 pos, Quaternion rot){
		ByteConstructor bc = new ByteConstructor(44);
		bc.add (objectID);
		bc.add (authorityID);
		bc.add (type);
		bc.add (prefabIndex);
		bc.add (pos);
		bc.add (rot);
		return bc.getBytes ();
	}
	public static byte[] rigidbodySpawn(float mass, Vector3 linVel, Vector3 angVel){
		ByteConstructor bc = new ByteConstructor(28);
		bc.add (mass);
		bc.add (linVel);
		bc.add (angVel);
		return bc.getBytes ();
	}
	public static byte[] planetSpawn(float mass, Vector3 linVel, Vector3 angVel, int seed, float radius){
		ByteConstructor bc = new ByteConstructor(36);
		bc.add (rigidbodySpawn(mass,linVel,angVel));
		bc.add (seed);
		bc.add (radius);
		return bc.getBytes ();
	}
	#endregion


	public static void receiveObject(byte[] data, out IdentityAndTransform iat){
		ByteReceiver br = new ByteReceiver (data);
		var objectID = br.getInt ();
		int authorityID = br.getInt ();
		var type = (ObjectType)br.getInt ();
		var prefabIndex = br.getInt ();
		var pos = br.getVector3 ();
		var rot = br.getQuaternion ();

		iat = NetTransportManager.instance.spawnObjectSync (prefabIndex, authorityID, pos, rot, objectID);
		switch (type) {
		case ObjectType.Planet:
			var mass = br.getFloat ();
			var linVel = br.getVector3 ();
			var angVel = br.getVector3 ();
			var seed = br.getInt ();
			var radius = br.getFloat ();
			UniverseManager.instance.syncPlanet (iat, mass, linVel, angVel, seed, radius);
			break;
		case ObjectType.Rigidbody:
			var massR = br.getFloat ();
			var linVelR = br.getVector3 ();
			var angVelR = br.getVector3 ();
			makeRigidBody (iat.instance, massR, linVelR, angVelR);
			break;
		}  
	}
	private static void makeRigidBody(GameObject instance, float mass, Vector3 linVel, Vector3 angVel){
		Rigidbody rb = instance.GetComponent<Rigidbody> ();
		if (rb == null)
			rb = instance.AddComponent<Rigidbody> ();
		rb.mass = mass;
		rb.velocity = linVel;
		rb.angularVelocity = angVel;
	}
}
