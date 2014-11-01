using UnityEngine;

public class AmmoBoxScript : MonoBehaviour 
{
    public GUIText Target;
    public int GBullets;
    public int LBullets;

    void FixedUpdate()
    {
        Target.text = "";
    }

    public void OnLookEnter()
    {
        Target.text = "Press E";
        if (!Input.GetKeyDown("e")) return;

        Destroy(gameObject);
        Destroy(Target);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Player") return;

        var shootingScript = other.gameObject.GetComponent<ShootingScript>();
        shootingScript.gun.Ammo += GBullets;
        shootingScript.LightGun.Ammo += LBullets;
        Destroy(gameObject);
    }
}