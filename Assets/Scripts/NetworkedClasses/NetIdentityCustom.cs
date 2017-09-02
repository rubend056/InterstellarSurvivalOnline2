using UnityEngine;
using System;

public class NetIdentityCustom : MonoBehaviour{

	//[HideInInspector]
	public NetIdentityCustom(){}
	public NetIdentityCustom(int objIDL, int authorityIDL){
		objID = objIDL;
		AuthorityID = authorityIDL;
	}

	public const int size = 10;

	public int objID;
	public NetObjBytes.ObjectType type = NetObjBytes.ObjectType.Generic;

//	public bool localPlayer = false;

	private int authorityID = 0;
	public int AuthorityID{
		get{ return authorityID;}
		set{ changeAuthority (value);}
	}

	private bool hasAuthority = false;
	public bool HasAuthority{
		get{ return hasAuthority;}
	}


	public void changeAuthority(int authorityIDL){
		authorityID = authorityIDL;
		if (authorityIDL == NetTransportManager.instance.playerInfo.uniqueID)
			hasAuthority = true;
		else
			hasAuthority = false;
	}

//	public int prefabIndex = 0;

//	public bool isServer(){
//		return server;
//	}
//	public bool isClient(){
//		return client;
//	}
//	public bool hasAuthority(){
//		return authority;
//	}
//	public bool isLocalPlayer(){
//		return localPlayer;
//	}
}