using UnityEngine;

public class UI : MonoBehaviour
{
    private GameObject player;
    private GameManager manager;

    void Awake()
    {
        manager = GameObject.Find("_GameManager").GetComponent<GameManager>();
        player = manager.FindActivePlayer();
    }

    // ReSharper disable once UnusedMember.Local
    void OnGUI()
    {
    }
}
