using System.Collections;
using System.Linq;
using Player;
using UnityEngine;

namespace Networking
{
    public class NetworkScript : Photon.MonoBehaviour
    {
        //This is used to set the build version
        public string BuildVersion = "1.0";
        //Array to hold the spawnspots
        private SpawnSpot[] _spawnSpots;
        //Holds the local player gameobject
        private GameObject _myPlayerGo;

        void Awake()
        {
            //Connects to the server with the settings from "PhotonServerSettings" and has the variable BuildVersion as the required string
            PhotonNetwork.ConnectUsingSettings(BuildVersion);
            //Finds gameobjects with the SpawnSpot script attached and adds them to the spawnspots array
            _spawnSpots = FindObjectsOfType<SpawnSpot>();
        }

        void OfflineMode()
        {
            //Call this function in Awake for offline mode
            //Good for testing on local PC without latency
            PhotonNetwork.offlineMode = true;
            //ConnectUsingSettings won't work in offline mode
            //So we're manually calling the JoinOrCreateRoom function
            StartCoroutine(JoinOrCreateRoom());
        }

        void OnGUI()
        {
            //Check connection state..
            if (!PhotonNetwork.connected && !PhotonNetwork.connecting)
            {
                //We are currently disconnected
                GUILayout.Label("Connection status: " + PhotonNetwork.connectionStateDetailed);

                GUILayout.BeginVertical();
                if (GUILayout.Button("Connect"))
                {
                    //Connect using the PUN wizard settings (Self-hosted server or Photon cloud)
                    PhotonNetwork.ConnectUsingSettings(BuildVersion);
                }
                GUILayout.EndVertical();
            }
            else
            {
                //We're connected!
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

        private bool _receivedRoomList;

        void OnConnectedToPhoton()
        {
            //Starts the function when connected
            StartCoroutine(JoinOrCreateRoom());
        }

        void OnDisconnectedFromPhoton()
        {
            //When disconnected, make sure the room list gets received again
            _receivedRoomList = false;
        }

        IEnumerator JoinOrCreateRoom()
        {
            //If the room list isn't received within 2 seconds, timeout
            var timeOut = Time.time + 2;
            while (Time.time < timeOut && !_receivedRoomList)
            {
                //Makes sure it checks rooms for 2 seconds, so it doesn't instantly create a room
                yield return 0;
            }
            //We still didn't join any room: create one
            if (PhotonNetwork.room != null) yield break;
            var roomName = "TestRoom" + Application.loadedLevelName;
            PhotonNetwork.CreateRoom(roomName, new RoomOptions { maxPlayers = 4 }, null);
        }

        void OnJoinedRoom()
        {
            //Calls the SpawnPlayer function
            SpawnPlayer();
        }

        void PlayerRoleActivator(string playerRole, int playerSpawn)
        {
            //Instantiates player at relevant spawnspot
            _myPlayerGo = PhotonNetwork.Instantiate(playerRole, _spawnSpots[playerSpawn].transform.position, _spawnSpots[playerSpawn].transform.rotation, 0);
            _myPlayerGo.GetComponent<FPSInputController>().enabled = true;
            _myPlayerGo.GetComponent<ShootingScript>().enabled = true;
            _myPlayerGo.GetComponent<MouseLook>().enabled = true;
            _myPlayerGo.GetComponent<CharacterMotor>().enabled = true;
            _myPlayerGo.GetComponent<NetworkCharacter>().enabled = false;
            _myPlayerGo.transform.FindChild("MainCamera").gameObject.SetActive(true);
            print(playerRole);
        }

        void SpawnPlayer()
        {
            //This spawns the player, and gives it a role based on how many players are in the room
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

            ////Random spawn
            //SpawnSpot mySpawnSpot = _spawnSpots[Random.Range(0, _spawnSpots.Length)];
        }

        void OnReceivedRoomListUpdate()
        {
            print("We received a room list update, total rooms now: " + PhotonNetwork.GetRoomList().Length);


            var wantedRoomName = "TestRoom" + Application.loadedLevelName;
            foreach (var room in PhotonNetwork.GetRoomList().Where(room => room.name == wantedRoomName))
            {
                PhotonNetwork.JoinRoom(room.name);
                break;
            }
            _receivedRoomList = true;
        }
    }
}