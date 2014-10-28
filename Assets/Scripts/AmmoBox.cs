using UnityEngine;
using System.Collections;

public class AmmoBox : MonoBehaviour {

    public int gBullets;
    public int lBullets;
    private SpawnPoint spawnPoint;

    void Awake()
    {
        spawnPoint = GameObject.Find("SpawnPoint").GetComponent<SpawnPoint>();
    }

	void OnTriggerEnter (Collider other) {
        if (other.gameObject.tag == "Player")
        {
            spawnPoint.gAmmo += gBullets;
            spawnPoint.lAmmo += lBullets;
            Destroy(gameObject);
        }
	}
}
