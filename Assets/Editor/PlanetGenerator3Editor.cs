using UnityEngine;
using System.Collections;
using UnityEditor;


[CustomEditor (typeof (PlanetGenerator3))]
public class PlanetGenerator3Editor : Editor {



	public override void OnInspectorGUI() {
		PlanetGenerator3 mapGen = (PlanetGenerator3)target;

		if (DrawDefaultInspector ()) {
			if (mapGen.autoUpdate) {
				mapGen.GenerateMap ();
			}
			//mapGen.checkSample ();
		}

		/*if (GUILayout.Button ("TakeSample")) {
			mapGen.takeSample ();
		}*/


		if (GUILayout.Button ("Generate")) {
			mapGen.GenerateMap ();
		}
			
	}

}