//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System.IO.Ports;
//using System;
//
//public class ArduinoComm : MonoBehaviour {
//	public string portName = "/dev/ttyUSB0";
//	string receiveString = "";
//	//enum DataType{Rotation,End};
//	SerialPort ardP;
//
//	// Use this for initialization
//	private Transform ownTransform;
//	void Start () {
//		ownTransform = this.transform;
//		ardP = new SerialPort(portName,115200);
//	}
//	
//	// Update is called once per frame
//	int offset = 0;
//	byte[] buffer = new byte[200];
//	void Update () {
//		if (!ardP.IsOpen) {
//			Debug.Log ("Port Open!");
//			ardP.Open ();
//		}
//		while (ardP.BytesToRead > 0) {
//			ardP.Read (buffer,offset,1);
//			char character = BitConverter.ToChar (buffer, offset);
//			offset ++;
//			if ( character != '\n') {
//				receiveString += character;
//			} else {
//				string[] values = receiveString.Split ('\t');
//				//if (values [0] == "ypr") {
//				if (values.Length == 4)
//					updateStaff (System.Convert.ToSingle (values [1]), System.Convert.ToSingle (values [2]), System.Convert.ToSingle (values [3]));
//				if (values.Length == 5)
//					updateStaff (System.Convert.ToSingle (values [1]), System.Convert.ToSingle (values [2]), System.Convert.ToSingle (values [3]), System.Convert.ToSingle (values [4]));
//				//}
//				receiveString = "";
//				buffer = new byte[200];
//				offset = 0;
//			}
//		}
//	}
//
//	Vector3 angOffset = Vector3.zero;
//	Vector3 currentEAngles = Vector3.zero;
//	Vector3 currentRot = Vector3.zero;
//	Vector3 rotOffset = Vector3.zero;
//	void updateStaff (float x, float y, float z){
//		currentEAngles = new Vector3(-z,x,y);
//		ownTransform.eulerAngles = currentEAngles -angOffset;
//	}
//	void updateStaff (float x, float y, float z, float w){
//		currentRot = new Quaternion(z,x,y,w).eulerAngles;
//		ownTransform.eulerAngles = currentRot - rotOffset;
//	}
//
//	public void setOffset(){
//		angOffset = currentEAngles;
//		rotOffset = currentRot;
//	}
//}
