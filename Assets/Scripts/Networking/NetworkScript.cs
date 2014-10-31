using System.Collections;
using System.Linq;
using Assets.Scripts.Player;
using UnityEngine;
using MonoBehaviour = Photon.MonoBehaviour;

namespace Assets.Scripts.Networking
{
    public class NetworkScript : MonoBehaviour
    {
        public string BuildVersion = "1.0";

        private SpawnSpot[] _spawnSpots;
        private GameObject _myPlayerGo;

        void Awake()
        {
            //PhotonNetwork.offlineMode = true;
            //StartCoroutine(JoinOrCreateRoom());
            PhotonNetwork.ConnectUsingSettings(BuildVersion);
            _spawnSpots = FindObjectsOfType<SpawnSpot>();
            //_spawnSpots = GameObject.FindObjectsOfType<SpawnSpot>();
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
            StartCoroutine(JoinOrCreateRoom());
        }

        void OnDisconnectedFromPhoton()
        {
            _receivedRoomList = false;
        }

        IEnumerator JoinOrCreateRoom()
        {
            var timeOut = Time.time + 2;
            while (Time.time < timeOut && !_receivedRoomList)
            {
                yield return 0;
            }
            //We still didn't join any room: create one
            if (PhotonNetwork.room != null) yield break;
            var roomName = "TestRoom" + Application.loadedLevelName;
            PhotonNetwork.CreateRoom(roomName, new RoomOptions { maxPlayers = 4 }, null);
        }

        void OnJoinedRoom()
        {
            SpawnPlayer();
        }

        void PlayerRoleActivator(string playerRole, int playerSpawn)
        {
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
            Debug.Log("We received a room list update, total rooms now: " + PhotonNetwork.GetRoomList().Length);

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