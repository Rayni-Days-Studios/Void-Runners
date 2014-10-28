using UnityEngine;
using System.Collections;

public class AmmoGUI : MonoBehaviour {
    private SpawnPoint spawnPoint;
    public GUIStyle ammoCount;

    void Awake()
    {
        spawnPoint = GameObject.Find("SpawnPoint").GetComponent<SpawnPoint>();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 0, 0), "" + spawnPoint.gAmmo + "   " + spawnPoint.lAmmo, ammoCount);
    }
}