using UnityEngine;
using System.Collections;

public class Tutorial_2B_Spawnscript : Photon.MonoBehaviour
{
   
    public Transform EdenPrefab;
    public Transform GunnerPrefab;
    public Transform ScoutPrefab;
    public Transform MonsterPrefab;
    static public int PlayerID;
    private Transform _playerPrefab;

    void OnJoinedRoom()
    {
        Spawnplayer();
    }

    void Spawnplayer()
    {
        switch (PlayerID)
        {
            case(0):
                _playerPrefab = EdenPrefab;
                break;
            case(1):
                _playerPrefab = GunnerPrefab;
                break;
            case(2):
                _playerPrefab = MonsterPrefab;
                break;
            case(3):
                _playerPrefab = ScoutPrefab;
                break;
            case(4):
                PlayerID = 0;
                break;
        }

        Vector3 pos = transform.position + new Vector3(Random.Range(-3,3),0,Random.Range(-3,3));
        PhotonNetwork.Instantiate(_playerPrefab.name, pos, transform.rotation, 0);
        PlayerID++;
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        Debug.Log("Clean up after player " + player);
  
    }

    void OnDisconnectedFromPhoton()
    {
        Debug.Log("Clean up a bit after server quit");
        
        /* 
        * To reset the scene we'll just reload it:
        */
        Application.LoadLevel(Application.loadedLevel);
    }

}