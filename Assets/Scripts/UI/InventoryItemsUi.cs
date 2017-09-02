using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class InventoryItemsUi : MonoBehaviour {

	//public PointerPick pp;
	private RectTransform invTrans;
	public GameObject menuItem;
	public Inventory inv;
	public BuildInventory binv;
	public bool buildShow = false;
	public int itemsPerRow = 4;
	public int itemsPerColumn = 2;
	public float separation = 10;
	private float width;
	private float height;
	// Use this for initialization
	void Awake(){
		destroyChildren ();
		invTrans = gameObject.GetComponent<RectTransform> ();
	}

	void destroyChildren(){
		int count = gameObject.transform.childCount;
		for (int i = 0; i<count;i++)
			GameObject.DestroyImmediate(gameObject.transform.GetChild (0).gameObject);
	}

	public void updateItems(){
		destroyChildren ();

		Rect rectangle = invTrans.rect;
		float xInt = (rectangle.width-((itemsPerRow+1)*separation)) / itemsPerRow;
		float yInt = (rectangle.height-((itemsPerColumn+1)*separation)) / itemsPerColumn;

		width = rectangle.width;
		height = rectangle.height;

		thing[] things;
		if (buildShow) {
			things = binv.blueprints.ToArray ();
		} else {
			things = inv.things.ToArray();
		}

		for (int i = 0; (i < (itemsPerRow * itemsPerColumn)) && i < things.Length; i++) {
			int y = i / itemsPerRow;
			int x = i - (y * itemsPerRow);

			GameObject itemInst = GameObject.Instantiate (menuItem, invTrans);
			RectTransform itemRectT = itemInst.GetComponent<RectTransform> ();
			//Rect itemRect = itemRectT.rect;
			float xMin = separation + (x * (xInt + separation));
			float xMax = rectangle.width - (separation + (x * (xInt + separation))) - xInt;
			float yMin = separation + (y * (yInt + separation));
			float yMax = rectangle.height - (separation + (y * (yInt + separation))) - yInt;
			itemRectT.anchoredPosition = new Vector2 (0, 0);
			itemRectT.localPosition = new Vector3 (0, 0, 0);
			itemRectT = UIUtility.changeDimentions (itemRectT, xMin, xMax, yMin, yMax);

			itemInst.name = things [i].name;
			itemRectT.GetChild (1).GetComponent<Text> ().text = things [i].name;
			if (buildShow)
				itemRectT.GetChild (2).GetComponent<Text> ().text = things [i].cost.ToString ();
			else
				itemRectT.GetChild (2).GetComponent<Text> ().text = things [i].count.ToString ();
		}

	}
}
