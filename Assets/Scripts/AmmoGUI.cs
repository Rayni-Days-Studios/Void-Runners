using UnityEngine;

[ExecuteInEditMode]
public class AmmoGUI : MonoBehaviour {

    private ShootingScript spawnPoint;
    public GUIStyle ammoCount;
 
    void Awake()
    {
        spawnPoint = GameObject.FindGameObjectWithTag("Player").GetComponent<ShootingScript>();
    }
    
    void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 0, 0), "" + spawnPoint.bulletGun.Ammo + "  " + spawnPoint.lightGun.Ammo, ammoCount);
    }
}