using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;


public class ButtonCustom : MonoBehaviour{

	public RectTransform rt;
	public MaskableGraphic buttonIcon;
	public Color buttonHColor = Color.white;

//	[Space(5)]
//	[Header("If it has a background?")]
//	public MaskableGraphic background;
//	public Color normalColor = Color.gray;
//	public Color highlightedColor = Color.white;

//	public int type = 0;
//	public PointerPick pp;
//	public DropDownUI ddui;
	public bool isWorldSpace = false;
	public UnityEvent onClicked;
	private Color startColor;
	private Rect rectangle;

	void Start(){
		rt = gameObject.GetComponent<RectTransform> ();
		if (buttonIcon == null)
			buttonIcon = gameObject.GetComponent<MaskableGraphic> ();
		if(!rt)rt = gameObject.GetComponent<RectTransform> ();
		startColor = buttonIcon.color;
//		if (!image && !text)
//		startColor = buttonIcon.color;
	}
	void Update(){
		if (!isWorldSpace) {
			rectangle = rt.rect;
			Vector3 mPos = Input.mousePosition;
			if (mPos.x > rt.position.x - (rectangle.width / 2) && mPos.x < rt.position.x + (rectangle.width / 2)) {
				if (mPos.y > rt.position.y - (rectangle.height / 2) && mPos.y < rt.position.y + (rectangle.height / 2)) {
					handleHit ();
				} else
					buttonIcon.color = startColor;
			} else
				buttonIcon.color = startColor;
		} else if (hit) {
			handleHit ();
			hit = false;
		} else buttonIcon.color = startColor;
	}

	void handleHit(){
		buttonIcon.color = buttonHColor;
		if (Input.GetMouseButtonUp (0)) {
			onClicked.Invoke ();
		}
	}

	public static void actionLoadScene(int scene){
		SceneManager.LoadScene (scene);
	}
	public static void actionLoadScene(string scene){
		SceneManager.LoadScene (scene);
	}
	public static void actionQuit(){
		Application.Quit ();
	}
	public static void Test(){
		DebugConsole.Log ("Yeah! :)");
	}

	private bool hit = false;
//	private bool hit2 = false;
	void beingHit(){
		hit = true;
	}


}
