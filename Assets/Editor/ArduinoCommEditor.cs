//using UnityEngine;
//using System.Collections;
//using UnityEditor;
//
//
//[CustomEditor (typeof (ArduinoComm))]
//public class ArduinoCommEditor : Editor {
//
//
//
//	public override void OnInspectorGUI() {
//		ArduinoComm comm = (ArduinoComm)target;
//		DrawDefaultInspector ();
//
//		if (GUILayout.Button ("Set Offset")) {
//			comm.setOffset ();
//		}
//
//	}
//
//}