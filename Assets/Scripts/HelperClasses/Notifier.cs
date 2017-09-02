using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Notifier : MonoBehaviour {
	public static Notifier instance;

	public Text notificationText;
	private string[] notStringArray;
	public const float holdTime = 2f;
	public const float releaseTime = 1f;

	void Awake(){
		instance = this;
	}

	void Start(){
		notificationText.gameObject.SetActive (false);
		this.notify ("Welcome :)",Color.green, 3);
	}

	List<TextGroup> textGroupL = new List<TextGroup>();
	void Update(){

		for (int i = 0; i < textGroupL.Count; i++) {
			var tg = textGroupL [i];

			tg.time += Time.deltaTime;
			tg.textInst.rectTransform.localPosition = Vector3.Lerp (tg.textInst.rectTransform.localPosition, tg.pos, 0.1f);				//Move towards toMove

			if (tg.time > tg.fadeTime * 2 + tg.holdTime) {
				GameObject.Destroy (tg.textInst.gameObject);
				textGroupL.Remove (tg);
				StartCoroutine( reorder ());
			} else if (tg.time > tg.holdTime + tg.fadeTime) {
				var color = tg.textInst.color;
				color.a = Mathf.MoveTowards (color.a, 0, (Time.deltaTime * (1-color.a + 0.03f)) * 3 / tg.fadeTime);		//Move alpha towards 0
				tg.textInst.color = color;
			} else if (tg.time > tg.fadeTime) {
				var color = tg.textInst.color;
				color.a = 1;
				tg.textInst.color = color;
			} else {
				var color = tg.textInst.color;
				color.a = Mathf.MoveTowards (color.a, 1, (Time.deltaTime * (1-color.a)) * 3 / tg.fadeTime);	//Move alpha towards 1
				tg.textInst.color = color;
			}
		}
	}

	
	public void notify(string text, Color color, float holdTime){
		var textObjectInst = GameObject.Instantiate(notificationText.gameObject, notificationText.rectTransform.parent);
		var notTextInst = textObjectInst.GetComponent<Text> ();
		notTextInst.text = text;
//		Debug.Log(notTextInst.rectTransform.sizeDelta.ToString ());
		notTextInst.gameObject.SetActive (true);
		color.a = 0;
		notTextInst.color = color;
		notTextInst.rectTransform.sizeDelta.ToString ();

		var tg = new TextGroup();
		tg.textObject = textObjectInst;
		tg.textInst = notTextInst;
		tg.pos = notificationText.rectTransform.localPosition;
		tg.fadeTime = releaseTime;
		tg.holdTime = holdTime;
		textGroupL.Add(tg);
		StartCoroutine(reorder ());
	}

	public void notify(string text, Color color){
		notify (text, color, holdTime);
	}

	public void notify(string text){
		notify (text, Color.green);
	}

//	private IEnumerator holdAndOut( RectTransform textTrans, Text textInst, float holdTime, float releaseTime, Vector3 toMove){
//		float time = Time.time;
//
//
//	}

//	private int findTextInst(GameObject textGo){
//		int instID = textGo.GetInstanceID ();
//		for (int i = 0; i < textGroup.Count; i++) {
//			if (textGroup [i].textObject.GetInstanceID () == instID) {
//				return i;
//			}
//		}
//		return -1;
//	}
//
	private IEnumerator reorder(){
		Vector3 lastPos = notificationText.rectTransform.localPosition;
		yield return new WaitForEndOfFrame ();
		int count = textGroupL.Count-1;
		for (int i = count; i >= 0; i--) {
			if (i == count)
				textGroupL [i].pos = lastPos;
			else{
				lastPos.y -= textGroupL [i+1].textObject.GetComponent<RectTransform> ().rect.height;
				textGroupL [i].pos = lastPos;
			}
		}
	}

	public void clear(){
		StopAllCoroutines ();
		textGroupL.Clear ();
	}
}

public class TextGroup{
//	public Coroutine cr;
	public GameObject textObject;
	public Text textInst;
	public Vector3 pos = new Vector3();
	public float time = 0;
	public float holdTime;
	public float fadeTime;
}
