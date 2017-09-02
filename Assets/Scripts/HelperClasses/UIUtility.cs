using UnityEngine;
using UnityEngine.UI;

public class UIUtility{
	public static RectTransform changeDimentions(RectTransform rt, float left, float right, float up, float down){
		rt.offsetMin = new Vector2 (left, down);
		rt.offsetMax = new Vector2 (-right, -up);
		return rt;
	}
}
