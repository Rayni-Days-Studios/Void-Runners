using UnityEngine;

[ExecuteInEditMode]
public class AmmoGUI : MonoBehaviour
{

    private ShootingScript shootingScript;
    public GUIStyle AmmoCount;

    private void Awake()
    {
        // Ensures that it's only the local player's shootingscript it finds
        shootingScript = GameObject.Find("_GameManager").GetComponent<NetworkScript>().MyPlayerGo.GetComponent<ShootingScript>();
    }

    private void OnGUI()
    {
        if (shootingScript.UsePrimaryGun)
        {
            GUILayout.Label("Total Ammo: " + shootingScript.LightGun.TotalAmmo, AmmoCount);
            GUILayout.Label("Loaded Ammo: " + shootingScript.LightGun.LoadedAmmo, AmmoCount);
        }
        else if (!shootingScript.UsePrimaryGun)
        {
            GUILayout.Label("Total Ammo: " + shootingScript.BulletGun.TotalAmmo, AmmoCount);
            GUILayout.Label("Loaded Ammo: " + shootingScript.BulletGun.LoadedAmmo, AmmoCount);
        }
    }
}