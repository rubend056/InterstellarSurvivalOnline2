using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Gravity : MonoBehaviour {

	//private GameObject[] planets;
	public float multFactor = 1;
	public float forceP;
//	public float distance;
	public const float gravityConst = 0.0000000000667408f * 10000000;

	Transform thisTransform;
	Rigidbody thisRigidBody;
	public Rigidbody[] planetInfoArray;

	void Start(){
		planetInfoArray = new Rigidbody[0];
		thisTransform = gameObject.transform;
		thisRigidBody = gameObject.GetComponent (typeof(Rigidbody)) as Rigidbody;
	}
	// Update is called once per frame
	int r = 61;
	void Update () {
		if (r > 60) {
			updatePlanets ();
			r = 0;
		}r++;
		forceP = 0;
		foreach (Rigidbody rb in planetInfoArray) {

			float distSqr = (rb.transform.position - thisTransform.position).sqrMagnitude;
			Vector3 direction = (rb.transform.position - thisTransform.position).normalized;

			float force = Time.deltaTime * 60 * getGravityForce(rb.mass, thisRigidBody.mass, distSqr)/*Sphere.getFictionalForce(gravityMultiplier.multiplier * planetR.rb.mass,distance) * divisionFactor*/;
			Vector3 forceVector = direction * force * multFactor;

			thisRigidBody.AddForce (forceVector, ForceMode.Force);

			forceP += force;
		}
	}

	public void updatePlanets(){
		
		GameObject[] planets = GameObject.FindGameObjectsWithTag("planetRB");
		GameObject[] stars = GameObject.FindGameObjectsWithTag ("star");


		List<GameObject> planetsL = new List<GameObject> ();		
		planetsL.AddRange (planets);
		planetsL.AddRange (stars);

		if (planetsL.Contains (gameObject))//If any of these gameObjects is youself, then remove yourself
			planetsL.Remove (gameObject);

		planets = planetsL.ToArray ();

		planetInfoArray = new Rigidbody[planets.Length];
		for (int i = 0; i < planetInfoArray.Length; i++) {
			

			planetInfoArray[i] = planets [i].GetComponent<Rigidbody> ();

//			planetR.distSqr = (planets[i].transform.position - thisTransform.position).sqrMagnitude;
//			Vector3 direction = (planetR.rb.transform.position - thisTransform.position).normalized;
//			planetR.dir = direction;

//			planetInfoArray [i] = planetR;
		}
		//waitTime = /*Mathf.Clamp (2/((thisRigidBody.velocity.magnitude*0.01f)+1),0.1f,1f)*/0.01f;
	}

	public static float getOrbitSpeedByCentripetal(float mass, float centripetal, float radius){
		return Mathf.Sqrt ((centripetal * radius) / mass); //Inverse of getCentrifugalBySpeed function
	}

	public static float getCentrifugalBySpeed(float mass, float speed, float radius){
		return (mass * (speed * speed)) / radius;
	}

	public static float getGravityForce(float obj1Mass, float obj2Mass, float distanceSqrd){
		return gravityConst * ((obj1Mass * obj2Mass) / distanceSqrd);
	}

//	private class planetsGInfo{
//		public Rigidbody rb;
////		public Vector3 dir;
////		public float distSqr;
//	}
}
