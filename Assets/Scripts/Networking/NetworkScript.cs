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
     * What role does this third person get? 
     * The third role?
     * Nope. The second one.
     * Why? Because the roles are based on how many players are in the room.
     * Work on fixing this instead.
     * Also, don't use 4 tags for one thing. Bad move!
     */

    // This is used to set the build version
    public string buildVersion = "1.0";

    // Array to hold the spots that can be spawned in
    private List<SpawnSpot> spawnSpots;

    // Holds the local player gameobject
    private GameObject myPlayerGo;

    void Awake()
    {
        // Connects to the server with the settings from "PhotonServerSettings" and has the variable BuildVersion as the required string
        PhotonNetwork.ConnectUsingSettings(buildVersion);
        // Finds gameobjects with the SpawnSpot script attached and adds them to the spawnspots array
        spawnSpots = FindObjectsOfType<SpawnSpot>().ToList();
    }

    void OfflineMode()
    {
        // Call this function in Awake for offline mode
        // Good for testing on local PC without latency
        PhotonNetwork.offlineMode = true;

        // ConnectUsingSettings won't work in offline mode
        // So we're manually calling the JoinOrCreateRoom function
        StartCoroutine(JoinOrCreateRoom());
    }

    void OnGUI()
    {
        // Check connection state..
        if (!PhotonNetwork.connected && !PhotonNetwork.connecting)
        {
            // We are currently disconnected
            GUILayout.Label("Connection status: " + PhotonNetwork.connectionStateDetailed);

            GUILayout.BeginVertical();
            if (GUILayout.Button("Connect"))
            {
                // Connect using the PUN wizard settings (Self-hosted server or Photon cloud)
                PhotonNetwork.ConnectUsingSettings(buildVersion);
            }
            GUILayout.EndVertical();
        }
        else
        {
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
        // Calls the SpawnPlayer function
        SpawnPlayer();
    }
 
    void PlayerRoleActivator(string playerRole, int playerSpawn)
    { 
        // Instantiates player at relevant spawnspot
        myPlayerGo = PhotonNetwork.Instantiate(playerRole, spawnSpots[playerSpawn].transform.position, spawnSpots[playerSpawn].transform.rotation, 0);
        myPlayerGo.GetComponent<FPSInputController>().enabled = true;
        myPlayerGo.GetComponent<ShootingScript>().enabled = true;
        myPlayerGo.GetComponent<MouseLook>().enabled = true;
        myPlayerGo.GetComponent<CharacterMotor>().enabled = true;
        myPlayerGo.GetComponent<NetworkCharacter>().enabled = false;
        myPlayerGo.transform.FindChild("MainCamera").gameObject.SetActive(true);
        print(playerRole);
    }

    void SpawnPlayer()
    {
        // This spawns the player, and gives it a role based on how many players are in the room
        switch (PhotonNetwork.countOfPlayers)
        {
            case(1):
                PlayerRoleActivator("Eden", 0);
                break;
            case (2):
                PlayerRoleActivator("Gunner", 1);
                break;
            case (3):
                PlayerRoleActivator("Monster", 2);
                break;
            case (4):
                PlayerRoleActivator("Scout", 3);
                break;
        }

        //// Random spawn
        //SpawnSpot mySpawnSpot = _spawnSpots[Random.Range(0, _spawnSpots.Length)];
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