﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkScript : Photon.MonoBehaviour
{
    // This is used to set the build version
    public string BuildVersion = "1.0";
    public GameObject StandbyCamera;
    public GameObject DirLight;
    public GUIStyle MyButtonStyle;
    public GUIStyle MyTextStyle;
    public GUIStyle MyTextFieldStyle;

    private List<string> chatMessages;
    public int MaxChatMessages = 5;

    private bool hasPickedRole;
    private bool connecting;
    private bool receivedRoomList;
    private bool edenSpawned;
    private bool gunnerSpawned;
    private bool scoutSpawned;
    private bool monsterSpawned;

    private GameObject edenSpawnspot;
    private GameObject gunnerSpawnspot;
    private GameObject scoutSpawnspot;
    private GameObject monsterSpawnspot;

    // Holds the local player gameobject
    [NonSerialized]
    public GameObject MyPlayerGo;

    void Start()
    {
        edenSpawnspot = GameObject.Find("EdenSpawn");
        gunnerSpawnspot = GameObject.Find("GunnerSpawn");
        scoutSpawnspot = GameObject.Find("ScoutSpawn");
        monsterSpawnspot = GameObject.Find("MonsterSpawn");

        PhotonNetwork.player.name = PlayerPrefs.GetString("Username", "DefaultName");
        chatMessages = new List<string>();
    }

    void OnDestroy()
    {
        PlayerPrefs.SetString("Username", PhotonNetwork.player.name);
    }

    public void AddChatMessage(string message)
    {
        GetComponent<PhotonView>().RPC("AddChatMessage_RPC", PhotonTargets.AllBuffered, message);
    }

    [RPC]
    void AddChatMessage_RPC(string message)
    {
        while (chatMessages.Count >= MaxChatMessages)
        {
            chatMessages.RemoveAt(0);
        }
        chatMessages.Add(message);
    }

    void Connect()
    {
        PhotonNetwork.ConnectUsingSettings(BuildVersion);
    }

    void OnGUI()
    {
        // Check connection state..
        if (!PhotonNetwork.connected && !PhotonNetwork.connecting)
        {
            GUILayout.BeginArea( new Rect(0, 0, Screen.width, Screen.height) );
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();
            GUILayout.Label("Username: ", MyTextFieldStyle);
            PhotonNetwork.player.name = GUILayout.TextField("", MyTextFieldStyle);
			GUILayout.EndHorizontal();

            if (GUILayout.Button("Single Player", MyButtonStyle))
            {
				connecting = true;
				PhotonNetwork.offlineMode = true;
			    StartCoroutine(JoinOrCreateRoom());
			}

            if (GUILayout.Button("Multi Player", MyButtonStyle))
            {
				connecting = true;
				Connect();
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.EndArea();

            // We are currently disconnected
            GUILayout.Label("Connection status: " + PhotonNetwork.connectionStateDetailed, MyTextStyle);
        }
        else
        {
            if (PhotonNetwork.connected == true && connecting == false)
            {

                if (hasPickedRole)
                {
                    // We are fully connected, make sure to display the chat box.
                    GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
                    GUILayout.BeginVertical();
                    GUILayout.FlexibleSpace();

                    foreach (string msg in chatMessages)
                    {
                        GUILayout.Label(msg, MyTextStyle);
                    }

                    GUILayout.EndVertical();
                    GUILayout.EndArea();
                }
                else
                {
                    // Player has not yet selected a role.

                    GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginVertical();
                    GUILayout.FlexibleSpace();

                    if (!edenSpawned)
                        if(GUILayout.Button("Eden", MyButtonStyle))
                        {
                            GetComponent<PhotonView>().RPC("UsedRole", PhotonTargets.AllBuffered, "Eden", true);
                            SpawnPlayer("Eden", edenSpawnspot);

                        }
                    if(!gunnerSpawned)
                        if (GUILayout.Button("Gunner", MyButtonStyle))
                        {
                            GetComponent<PhotonView>().RPC("UsedRole", PhotonTargets.AllBuffered, "Gunner", true);
                            SpawnPlayer("Gunner", gunnerSpawnspot);
                        }
                    if(!scoutSpawned)
                        if (GUILayout.Button("Scout", MyButtonStyle))
                        {
                            GetComponent<PhotonView>().RPC("UsedRole", PhotonTargets.AllBuffered, "Scout", true);
                            SpawnPlayer("Scout", scoutSpawnspot);
                        }
                    if(!monsterSpawned)
                        if (GUILayout.Button("Monster", MyButtonStyle))
                        {
                            GetComponent<PhotonView>().RPC("UsedRole", PhotonTargets.AllBuffered, "Monster", true);
                            SpawnPlayer("Monster", monsterSpawnspot);
                        }

                    GUILayout.FlexibleSpace();
                    GUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.EndArea();
                }
            }
            // We're connected!
            GUILayout.Label("Connection status: " + PhotonNetwork.connectionStateDetailed, MyTextStyle);
            if (PhotonNetwork.room != null)
            {
                GUILayout.Label("Room: " + PhotonNetwork.room.name, MyTextStyle);
                GUILayout.Label("Players: " + PhotonNetwork.room.playerCount + "/" + PhotonNetwork.room.maxPlayers, MyTextStyle);
            }
            else
            {
                GUILayout.Label("Not inside any room", MyTextStyle);
            }

            GUILayout.Label("Ping to server: " + PhotonNetwork.GetPing(), MyTextStyle);

        }
    }

    void OnConnectedToPhoton()
    {
        // Starts the function when connected
        StartCoroutine(JoinOrCreateRoom());
    }

    void OnDisconnectedFromPhoton()
    {
        // When disconnected, make sure the room list gets received again
        receivedRoomList = false;
    }

    IEnumerator JoinOrCreateRoom()
    {
        // If the room list isn't received within 2 seconds, timeout
        float timeOut = Time.time + 2;
        while (Time.time < timeOut && !receivedRoomList)
        {
            // Makes sure it checks rooms for 2 seconds, so it doesn't instantly create a room
            yield return 0;
        }
        // We still didn't join any room: create one
        if (PhotonNetwork.room != null) yield break;
            string roomName = "TestRoom" + Application.loadedLevelName;
            PhotonNetwork.CreateRoom(roomName, new RoomOptions { maxPlayers = 4 }, null);
    }

    void OnJoinedRoom()
    {
        print("Joined room");
        connecting = false;
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer other)
    {
        AddChatMessage(other.name + " has disconnected");
        //GetComponent<PhotonView>().RPC("UsedRole", PhotonTargets.AllBuffered, ****************, false);
    }

    void SpawnPlayer(string playerRole, GameObject playerSpawn)
    {
        hasPickedRole = true;
        AddChatMessage(PhotonNetwork.player.name + " has joined as " + playerRole);
        // Instantiates player at relevant spawnspot
        MyPlayerGo = PhotonNetwork.Instantiate(playerRole, playerSpawn.transform.position, playerSpawn.transform.rotation, 0);
        MyPlayerGo.GetComponent<FPSInputController>().enabled = true;
        MyPlayerGo.GetComponent<MouseLook>().enabled = true;
        MyPlayerGo.GetComponent<CharacterMotor>().enabled = true;
        MyPlayerGo.GetComponent<NetworkCharacter>().enabled = false;

        // Avoids nullreference when Scout and Monster gets spawned
        if (playerRole == "Eden" || playerRole == "Gunner") MyPlayerGo.GetComponent<Player>().enabled = true;

        // Disable the local player model to boost performance
        MyPlayerGo.transform.FindChild("Model").gameObject.SetActive(false);

        StandbyCamera.SetActive(false);
        //DirLight.SetActive(false);
        MyPlayerGo.transform.FindChild("MainCamera").gameObject.SetActive(true);
    }

    void OnReceivedRoomListUpdate()
    {
        print("We received a room list update, total rooms now: " + PhotonNetwork.GetRoomList().Length);
        string wantedRoomName = "TestRoom" + Application.loadedLevelName;
        foreach (RoomInfo room in PhotonNetwork.GetRoomList().Where(room => room.name == wantedRoomName))
        {
            PhotonNetwork.JoinRoom(room.name);
            break;
        }
        receivedRoomList = true;
    }

    [RPC] 
    public void UsedRole(string role, bool state) 
    {
        switch (role)
        {
            case "Eden":
                edenSpawned = state;
                break;
            case "Gunner":
                gunnerSpawned = state;
                break;
            case "Scout":
                scoutSpawned = state;
                break;
            case "Monster":
                monsterSpawned = state;
                break;
            default:
                Debug.LogError("UsedRole: Invalid role");
                break;
        }
    }
}