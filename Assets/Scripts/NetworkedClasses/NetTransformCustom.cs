using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

[RequireComponent(typeof(NetIdentityCustom))]
public class NetTransformCustom : MonoBehaviour{

	public Transform trans;
	[Range(0,30)]
	public int sendRate = 0;
	public NetTransportObjectSync.TransUpdateType updateType = NetTransportObjectSync.TransUpdateType.Transform;
	[HideInInspector]
	public Coroutine coroutine;
	[HideInInspector]
	public bool smoothing = true;
	public bool localUpdate = false;
	public bool pos = true, rot = true, sca = true;

	public NetTransformCustom(){}

	void Start(){
		if (trans == null)
			trans = gameObject.transform;
	}

	private Coroutine moveToCoroutineValue;
	public void moveTo(Vector3 value){
		if (moveToCoroutineValue != null) {
			StopCoroutine (moveToCoroutineValue);
			moveToCoroutineValue = null;
		}
		if (smoothing)
			moveToCoroutineValue = StartCoroutine (moveToCoroutine (value, 1 / (sendRate + 1), trans));
		else
			setPosition (value);
	}

	private IEnumerator moveToCoroutine(Vector3 value, float interpolTime, Transform trans){
		float interpol = (0.01f / interpolTime);
		interpol = Mathf.Clamp (interpol, 0.01f, 0.8f);
		while (trans.position != value) {
			setPosition (Vector3.Slerp (getPosition (), value, interpol * Time.deltaTime * 60));
			yield return null;
		}
	}

	private Coroutine rotateToCoroutineValue;
	public void rotateTo(Quaternion value){
		if (rotateToCoroutineValue != null) {
			StopCoroutine (rotateToCoroutineValue);
			rotateToCoroutineValue = null;
		}
		if (smoothing)
			rotateToCoroutineValue = StartCoroutine (rotateToCoroutine (value, 1 / (sendRate + 1), trans));
		else
			setRotation (value);
	}

	private IEnumerator rotateToCoroutine(Quaternion value, float interpolTime, Transform trans){
		float interpol = (0.01f / interpolTime);
		interpol = Mathf.Clamp (interpol, 0.01f, 0.5f);
		while (trans.rotation != value) {
			setRotation (Quaternion.Slerp (getRotation(), value, interpol * Time.deltaTime * 60));
			yield return null;
		}
	}

	public byte[] getTransformBytes(){
		ByteConstructor bc = new ByteConstructor ();
		bc.add (new bool[]{ sca, rot, pos });
		if (pos)
			bc.add (getPosition ());
		if (rot)
			bc.add (getRotation ());
		if (sca)
			bc.add (trans.localScale);
		return bc.getBytes ();
	}

	public void receiveTransform(ByteReceiver br){
		bool[] boolA = br.getBoolArray ();
		if (boolA [7])
			moveTo (br.getVector3 ());
		if (boolA [6])
			rotateTo (br.getQuaternion ());
		if (boolA [5])
			trans.localScale = br.getVector3 ();
	}

	#region get-set Functions
	private void setRotation(Quaternion value){
		if (localUpdate)
			trans.localRotation = value;
		else
			trans.rotation = value;
	}
	private void setPosition(Vector3 value){
		if (localUpdate)
			trans.localPosition = value;
		else
			trans.position = value;
	}
	public Quaternion getRotation(){
		if (localUpdate)
			return trans.localRotation;
		else
			return trans.rotation;
	}
	public Vector3 getPosition(){
		if (localUpdate)
			return trans.localPosition;
		else
			return trans.position;
	}
	#endregion

//	public void OnSlider(){
//		sendRate = (int)slider.value;
//	}


}