using UnityEngine;

[ExecuteInEditMode]
public class AmmoGUI : MonoBehaviour
{

    private Player shootingScript;
    public GUIStyle AmmoCount;

    private void Awake()
    {
        // Ensures that it's only the local player's shootingscript it finds
        shootingScript = GameObject.Find("_GameManager").GetComponent<NetworkScript>().MyPlayerGo.GetComponent<Player>();
    }

    private void OnGUI()
    {
        if (shootingScript.UseLightGun)
        {
            GUILayout.Label(shootingScript.LightGun.TotalAmmo + " - Total Ammo", AmmoCount);
            GUILayout.Label(shootingScript.LightGun.LoadedAmmo + " - Loaded Ammo", AmmoCount);
        }
        else if (!shootingScript.UseLightGun)
        {
            GUILayout.Label(shootingScript.BulletGun.TotalAmmo + " - Total Ammo", AmmoCount);
            GUILayout.Label(shootingScript.BulletGun.LoadedAmmo + " - Loaded Ammo", AmmoCount);
        }
    }
}