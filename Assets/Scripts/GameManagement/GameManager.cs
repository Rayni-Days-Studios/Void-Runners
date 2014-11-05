using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject FindActivePlayer()
    {
        return GameObject.Find("_GameManager").GetComponent<NetworkScript>().myPlayerGo;
    }
}
