using UnityEngine;

[ExecuteInEditMode]
public class AmmoGUI : MonoBehaviour
{

    private ShootingScript player;
    public  GUIStyle ammoCount;
    private ShootingScript shootScript;
    

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<ShootingScript>();
    }

    private void OnGUI()
    {
        if (!player.whichGun)
        {
            GUILayout.Label("LoadedAmmo: " + player.bulletGun.loadedAmmo, ammoCount);
            GUILayout.Label("Total ammo left: " + player.bulletGun.totalAmmo, ammoCount);
        }
        else
        {
            GUILayout.Label("LoadedAmmo: " + player.lightGun.loadedAmmo, ammoCount);
            GUILayout.Label("Total ammo left: " + player.lightGun.totalAmmo, ammoCount);
        }
    }

}