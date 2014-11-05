using UnityEngine;

[ExecuteInEditMode]
public class AmmoGUI : MonoBehaviour
{

    private ShootingScript spawnPoint;
    public GUIStyle ammoCount;

    private void Awake()
    {
        spawnPoint = GameObject.FindGameObjectWithTag("Player").GetComponent<ShootingScript>();
    }

    private void OnGUI()
    {
    }
}