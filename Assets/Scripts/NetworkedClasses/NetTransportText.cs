using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetTransportText : NetTransportDebug{

	[Space(5)]
	[Header("NetTexting")]
	public int maxPDispName = 10;
	public InputField nameField;
	public InputField ipField;
	public InputField sendField;

	public override void Update (){
		base.Update ();
		if (Input.GetKeyDown (KeyCode.Return)) {
			sendText ();
		}
	}

	public override void ReceiveDataEvent (DataType type, PlayerInfo playerInfo, byte[] data, int index = 0){
		base.ReceiveDataEvent (type, playerInfo, data, index);

		if (type == DataType.Text){
			ByteReceiver r = new ByteReceiver (data, index);
			string message = r.getString ();
			textReceived (playerInfo, message);
		}
	}

	public virtual void sendText(string text){
		logMessage (text, playerInfo.color);
		sendAllAdvanced (ByteHelper.Combine (ByteHelper.getBytes ((int)DataType.Text), ByteHelper.getBytes (text)));
	}

	public virtual void sendText(){
		if (!isConnected ()) {
			logMessage ("Not Connected", Color.red);
			sendField.ActivateInputField ();
			return;
		}
		if (sendField.text == "") {
			Notifier.instance.notify ("Nothing to Send :(", Color.red);
			return;
		}

		string text = sendField.text;
		sendField.text = "";
		string toPrint = String.Format("Me\t\t\t:{0}", text);
		//		for (int i = 0; i < maxPDispName-2; i++) {
		//			toPrint += " ";
		//		}
		logMessage (toPrint, playerInfo.color);
//		Notifier.instance.notify (String.Format("I said: {0}", text), playerInfo.color);
		byte[] textB = ByteHelper.getBytes (text);
		byte[] combined = ByteHelper.Combine (System.BitConverter.GetBytes ((int)DataType.Text), textB);
		sendField.ActivateInputField ();

		//SentTo Server/AllClients
		sendAllAdvanced(combined);
	}

	public virtual void textReceived(PlayerInfo pi, string message){
		string toLog1 = pi.name;
		if (message [0] == '/' && isServer()) {
			message = message.Remove (0, 1);
			string[] commands = message.Split (new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
			if (commands.Length == 0)
				return;
			switch (commands [0]) {
			case "changename":
				string oldName = pi.name;
				string newName = commands [1];
				changeName (pi.uniqueID, newName);
				sendText (String.Format ("{0} is now {1}", oldName, newName));
				break;
			case "changecolor":
				float r = (float)Convert.ToChar (commands [1]) / 255;
				float g = (float)Convert.ToChar (commands [2]) / 255;
				float b = (float)Convert.ToChar (commands [3]) / 255;
				var color = new Color (r, g, b);
				changeColor (pi.uniqueID, color);
				sendText (String.Format ("{0} changed color r:{1} g:{2} b:{3}", new string[]{pi.name, commands[1], commands[2], commands[3]}));
				break;
			}
			updatePlayer (pi.uniqueID);
		} else { //Taken as a normal text message
			//Normalize player name *********************************
			if (toLog1.Length > maxPDispName) {
				toLog1 = toLog1.Substring (0, maxPDispName - 2);
				toLog1 += "..";
			} else
				while (toLog1.Length < maxPDispName)
					toLog1 += " ";
			//*******************************************************
			toLog1 += "\t:";
			toLog1 += message;
			string toLog2 = String.Format ("{0} said: {1}", pi.name, message);
			textReceivedFormated (toLog1, toLog2, pi.color);
		}
	}

	public virtual void textReceivedFormated(string message1, string message2, Color color){
		logMessage (message1, color);
		Notifier.instance.notify (message2, color);
	}

	public void nameChanged(){
		name = nameField.text;
	}
	public void ipChanged(){
		ipAddress = ipField.text;
	}

}
