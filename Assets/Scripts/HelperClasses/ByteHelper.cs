using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Text;

public class ByteHelper
{
	#region Raw Byte Handler Functions
	public static byte[] Combine(byte[] first, byte[] second)
	{
		byte[] ret = new byte[first.Length + second.Length];
		Buffer.BlockCopy(first, 0, ret, 0, first.Length);
		Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
		return ret;
	}

	public static byte[] Combine(byte[] first, byte[] second, byte[] third)
	{
		byte[] ret = new byte[first.Length + second.Length + third.Length];
		Buffer.BlockCopy(first, 0, ret, 0, first.Length);
		Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
		Buffer.BlockCopy(third, 0, ret, first.Length + second.Length, third.Length);
		return ret;
	}

	public static byte[] Combine(params byte[][] arrays)
	{
		byte[] ret = new byte[arrays.Sum(x => x.Length)];
		int offset = 0;
		foreach (byte[] data in arrays)
		{
			Buffer.BlockCopy(data, 0, ret, offset, data.Length);
			offset += data.Length;
		}
		return ret;
	}

	public static byte[] RemoveBefore(byte[] source, int index){
		int length = source.Length - index;
		byte[] final = new byte[length];
		for (int i = 0; i < length; i++) {
			final [i] = source [i + index];
		}
		return final;
	}

	#endregion

	#region Unity Classes Byte Functions

	//Vector3
	public static byte[] vector3Bytes(Vector3 var){
		ByteConstructor bc = new ByteConstructor (12);
		bc.add (var.x);
		bc.add (var.y);
		bc.add (var.z);
		return bc.getBytes();
	}
	public static Vector3 getVector3(byte[] data, int startIndex){
		ByteReceiver br = new ByteReceiver (data, startIndex);
		float x = br.getFloat();
		float y = br.getFloat();
		float z = br.getFloat();
		return new Vector3 (x, y, z);
	}

	//Quaternion
	public static byte[] quaternionBytes(Quaternion val){
		ByteConstructor bc = new ByteConstructor (16);
		bc.add (val.x);
		bc.add (val.y);
		bc.add (val.z);
		bc.add (val.w);
		return bc.getBytes();
	}
	public static Quaternion getQuaternion(byte[] data, int startIndex){
		ByteReceiver br = new ByteReceiver (data, startIndex);
		float x = br.getFloat();
		float y = br.getFloat();
		float z = br.getFloat();
		float w = br.getFloat();
		return new Quaternion (x, y, z, w);
	}

	//Color
	public static byte[] colorBytes(Color var){
		ByteConstructor bc = new ByteConstructor (16);
		bc.add (var.r);
		bc.add (var.g);
		bc.add (var.b);
		bc.add (var.a);
		return bc.getBytes();
	}
	public static Color getColor(byte[] data, int startIndex){
		ByteReceiver br = new ByteReceiver (data, startIndex);
		float r = br.getFloat();
		float g = br.getFloat();
		float b = br.getFloat();
		float a = br.getFloat();
		return new Color (r, g, b, a);
	}

	//Rigidbody
	public static byte[] rigidbodyBytes(Rigidbody var){
		ByteConstructor bc = new ByteConstructor (28);
		bc.add (var.mass);
		bc.add (var.velocity);
		bc.add (var.angularVelocity);
		return bc.getBytes ();
	}
	public static Rigidbody getRigidbody(byte[] data, int startIndex){
		ByteReceiver br = new ByteReceiver (data, startIndex);
		Rigidbody rb = new Rigidbody ();
		rb.mass = br.getFloat ();
		rb.velocity = br.getVector3 ();
		rb.angularVelocity = br.getVector3 ();
		return rb;
	}

	#endregion

	#region StandardTypes

	public static byte[] getBytes(bool[] var){
		return new byte[]{ByteHelper.boolToByte (var)};
	}
	public static byte[] getBytes(bool var){
		return (BitConverter.GetBytes (var));
	}
	public static byte[] getBytes(uint var){
		return (BitConverter.GetBytes (var));
	}
	public static byte[] getBytes(ushort var){
		return (BitConverter.GetBytes (var));
	}
	public static byte[] getBytes(float var){
		return (BitConverter.GetBytes (var));
	}
	public static byte[] getBytes(long var){
		return (BitConverter.GetBytes (var));
	}
	public static byte[] getBytes(int var){
		return (BitConverter.GetBytes (var));
	}
	public static byte[] getBytes(short var){
		return (BitConverter.GetBytes (var));
	}
	public static byte[] getBytes(double var){
		return (BitConverter.GetBytes (var));
	}
	public static byte[] getBytes(char var){
		return (BitConverter.GetBytes (var));
	}
	public static byte[] getBytes(ulong var){
		return(BitConverter.GetBytes (var));
	}

	public static byte[] getBytes(string text){
		return ByteHelper.Combine(
			BitConverter.GetBytes((ushort)text.Length),
			System.Text.Encoding.ASCII.GetBytes (text)
		);
	}
	public static byte[] getBytes(int[] intArray){
		byte[] bytes = BitConverter.GetBytes ((ushort)intArray.Length);
		for (int i = 0; i < intArray.Length; i++) {
			bytes = ByteHelper.Combine (bytes, BitConverter.GetBytes (intArray [i]));
		}
		return bytes;
	}
	#endregion

	//Bool
	public static byte boolToByte(bool[] source){
		byte result = 0;
		// This assumes the array never contains more than 8 elements!
		int index = 8 - source.Length;

		// Loop through the array
		foreach (bool b in source){
			// if the element is 'true' set the bit at that position
			if (b)
				result |= (byte)(1 << (7 - index));

			index++;
		}

		return result;
	}
	public static bool[] byteToBool(byte b)
	{
		// prepare the return result
		bool[] result = new bool[8];

		// check each bit in the byte. if 1 set to true, if 0 set to false
		for (int i = 0; i < 8; i++)
			result[i] = (b & (1 << i)) == 0 ? false : true;

		// reverse the array
		Array.Reverse(result);

		return result;
	}


	// to convert from anything to byte               	System.BitConverter. GetBytes (value);
	// to convert from byte to anything					System.BitConverter. Something();
	// decoding from bytes to text 						System.Text.Encoding.ASCII.GetString(byte[]);
	// encoding from text to byte						System.Text.Encoding.ASCII.GetBytes(text);

}

public class ByteConstructor{
	public List<byte> bytesList = new List<byte> ();

	//Performance variables
	public byte[] bytes;
	private bool knownSize = false;
	int index = 0;

	public ByteConstructor(){}
	public ByteConstructor(int size){
		bytes = new byte[size];
		knownSize = true;
	}
	public ByteConstructor(byte[] bytes){
		bytesList.AddRange (bytes);
	}
	public void add(byte[] staff){
		if (knownSize) {
//			if (staff.Length > bytes.Length - index + 1) {
//				Debug.Log ("Wrong ByteConstructor size");
//				return;
//			}
			Buffer.BlockCopy (staff, 0, bytes, index, staff.Length);
			index += staff.Length;
		} else 
			bytesList.AddRange (staff);
	}
	public byte[] getBytes(){
		if (knownSize)
			return bytes;
		else return bytesList.ToArray ();
	}

	#region Add Specific
	public void add(bool[] var){
		add (ByteHelper.getBytes (var));
	}
	public void add(bool var){
		add (ByteHelper.getBytes (var));
	}
	public void add(uint var){
		add (ByteHelper.getBytes (var));
	}
	public void add(ushort var){
		add (ByteHelper.getBytes (var));
	}
	public void add(float var){
		add (ByteHelper.getBytes (var));
	}
	public void add(long var){
		add (ByteHelper.getBytes (var));
	}
	public void add(int var){
		add (ByteHelper.getBytes (var));
	}
	public void add(short var){
		add (ByteHelper.getBytes (var));
	}
	public void add(double var){
		add (ByteHelper.getBytes (var));
	}
	public void add(char var){
		add (ByteHelper.getBytes (var));
	}
	public void add(ulong var){
		add (ByteHelper.getBytes (var));
	}


	public void add(Vector3 var){
		add (ByteHelper.vector3Bytes (var));
	}
	public void add(Quaternion var){
		add (ByteHelper.quaternionBytes (var));
	}
	public void add(string var){
		add (ByteHelper.getBytes (var));
	}
	public void add(Rigidbody var){
		add (var.velocity);
		add (var.angularVelocity);
	}

	#endregion

	public void addLevels(int[] levels){
		for (int i = 0; i < levels.Length; i++) {
			add(ByteHelper.getBytes(levels[i]));
		}
	}
	public byte[] addLevels(int[] levels, byte[] toSend){
		addLevels (levels);
		add(toSend);
		return bytesList.ToArray ();
	}

}

public class ByteReceiver{
	public byte[] data;
	public int index = 0;
	public ByteReceiver(byte[] dataL){
		data = dataL;
	}
	public ByteReceiver(byte[] dataL, int start){
		data = dataL;
		index = start;
	}
	public byte[] clean(){
		data = ByteHelper.RemoveBefore (data, index);
		index = 0;
		return data;
	}
	public byte getByte(){
		var value = data[index];
		index++;
		return value;
	}

	#region Standard Types
	public bool[] getBoolArray(){
		return ByteHelper.byteToBool (getByte ());
	}
	public bool getBool(){
		return ByteHelper.byteToBool (getByte ())[7];
	}
	public short getShort(){
		var value = BitConverter.ToInt16 (data, index);
		index += 2;
		return value;
	}
	public int getInt(){
		var value = BitConverter.ToInt32(data,index);
		index+=4;
		return value;
	}
	public float getFloat(){
		var value= BitConverter.ToSingle(data,index);
		index+=4;
		return value;
	}
	public double getDouble(){
		var value= BitConverter.ToDouble(data,index);
		index+=8;
		return value;
	}
	public Vector3 getVector3(){
		var vector3 = ByteHelper.getVector3 (data, index);
		index += 12;
		return vector3;
	}
	public Quaternion getQuaternion(){
		var value = ByteHelper.getQuaternion (data, index);
		index += 16;
		return value;
	}
	public string getString(){
		int length = (ushort)getShort ();
		string toReturn = System.Text.Encoding.ASCII.GetString(data, index, length);
		index += length;
		return toReturn;
	}
	public int[] getIntArray(){
		int length = (ushort)getShort ();
		int[] array = new int[length];
		for (int i = 0; i < length; i++) {
			array[i] = getInt ();
		}
		return array;
	}
	#endregion

	public Color getColor(){
		Color color = ByteHelper.getColor (data, index);
		index += 16;
		return color;
	}
	public Rigidbody getRigidBody(){
		var rb = ByteHelper.getRigidbody (data, index);
		index += 28;
		return rb;
	}
}