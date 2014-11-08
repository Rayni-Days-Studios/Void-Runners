using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkScript : Photon.MonoBehaviour
{
    /* Work on a better spawn system instead, not just the spawnspots. 
     * Right now the spawn system is broken. If someone joins, they get the first role. 
     * Now say someone else joins, that person gets the second role. 
     * Now, the first player leaves, and another person joins. 
     * What role does this third person get? The third role? Nope. The second one.
     * Why? Because the roles are based on how many players are in the room.
     * Work on fixing this instead. */

    // This is used to set the build version
    public string BuildVersion = "1.0";
    // Array to hold the spots that can be spawned in
    private List<SpawnSpot> spawnSpots;
    public GameObject StandbyCamera;

    private List<string> chatMessages;
    public int MaxChatMessages = 5;

    private bool scoutSpawned;
    private bool monsterSpawned;
    private bool gunnerSpawned;
    private bool edenSpawned;

    private bool hasPickedRole;
    private bool connecting;

    // Holds the local player gameobject
    [NonSerialized]
    public GameObject MyPlayerGo;

    void Start()
    {
        // Finds gameobjects with the SpawnSpot script attached and adds them to the spawnspots array
        spawnSpots = FindObjectsOfType<SpawnSpot>().ToList();
        PhotonNetwork.player.name = PlayerPrefs.GetString("Username", "Awesome Dude");
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
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
        
        // Check connection state..
        if (!PhotonNetwork.connected && !PhotonNetwork.connecting)
        {

            GUILayout.BeginArea( new Rect(0, 0, Screen.width, Screen.height) );
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Username: ");
			PhotonNetwork.player.name = GUILayout.TextField(PhotonNetwork.player.name);
			GUILayout.EndHorizontal();

			if( GUILayout.Button("Single Player") ) {
				connecting = true;
				PhotonNetwork.offlineMode = true;
			    StartCoroutine(JoinOrCreateRoom());
			}

			if( GUILayout.Button("Multi Player") ) {
				connecting = true;
				Connect();
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.EndArea();

            // We are currently disconnected
            GUILayout.Label("Connection status: " + PhotonNetwork.connectionStateDetailed);
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
                        GUILayout.Label(msg);
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
                        if(GUILayout.Button("Eden"))
                        {
                            SpawnPlayer("Eden", 0);
                            edenSpawned = true;
                        }
                    if(!gunnerSpawned)
                        if (GUILayout.Button("Gunner"))
                        {
                            SpawnPlayer("Gunner", 1);
                            gunnerSpawned = true;
                        }
                    if(!scoutSpawned)
                        if (GUILayout.Button("Scout"))
                        {
                            SpawnPlayer("Scout", 2);
                            scoutSpawned = true;
                        }
                    if(!monsterSpawned)
                        if (GUILayout.Button("Monster"))
                        {
                            SpawnPlayer("Monster", 3);
                            monsterSpawned = true;
                        }

                    GUILayout.FlexibleSpace();
                    GUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.EndArea();
                }
            }
            // We're connected!
            GUILayout.Label("Connection status: " + PhotonNetwork.connectionStateDetailed);
            if (PhotonNetwork.room != null)
            {
                GUILayout.Label("Room: " + PhotonNetwork.room.name);
                GUILayout.Label("Players: " + PhotonNetwork.room.playerCount + "/" + PhotonNetwork.room.maxPlayers);
            }
            else
            {
                GUILayout.Label("Not inside any room");
            }

            GUILayout.Label("Ping to server: " + PhotonNetwork.GetPing());

        }
    }
    private bool receivedRoomList;

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

    void SpawnPlayer(string playerRole, int playerSpawn)
    {
        hasPickedRole = true;
        AddChatMessage("Spawning player: " + PhotonNetwork.player.name);
        // This spawns the player, and gives it a role based on how many players are in the room
        // Instantiates player at relevant spawnspot
        MyPlayerGo = PhotonNetwork.Instantiate(playerRole, spawnSpots[playerSpawn].transform.position, spawnSpots[playerSpawn].transform.rotation, 0);
        MyPlayerGo.GetComponent<FPSInputController>().enabled = true;
        MyPlayerGo.GetComponent<MouseLook>().enabled = true;
        MyPlayerGo.GetComponent<CharacterMotor>().enabled = true;
        MyPlayerGo.GetComponent<NetworkCharacter>().enabled = false;

        // Avoids nullreference when Scout and Monster gets spawned
        if (playerRole == "Eden" || playerRole == "Gunner") MyPlayerGo.GetComponent<Player>().enabled = true;

        // Disable the local player model to boost performance
        MyPlayerGo.transform.FindChild("Model").gameObject.SetActive(false);

        StandbyCamera.SetActive(false);
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
}