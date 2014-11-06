using UnityEngine;

[ExecuteInEditMode]
public class AmmoGUI : MonoBehaviour
{

    private ShootingScript shootingScript;
    public GUIStyle ammoCount;

    private void Awake()
    {
        shootingScript = GameObject.FindGameObjectWithTag("Player").GetComponent<ShootingScript>();
    }

    private void OnGUI()
    {
        GUILayout.Label("Total Ammo: " + shootingScript.lightGun.TotalAmmo, ammoCount);
        GUILayout.Label("Loaded Ammo: " + shootingScript.lightGun.LoadedAmmo, ammoCount);
    }
}