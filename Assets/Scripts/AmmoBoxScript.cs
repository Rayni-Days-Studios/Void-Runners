using UnityEngine;

public class AmmoBoxScript : MonoBehaviour 
{
    public GUIText target;
    public int gBullets;
    public int lBullets;

    void FixedUpdate()
    {
        target.text = "";
    }

    public void OnLookEnter()
    {
        target.text = "Press E";
        if (!Input.GetKeyDown("e")) return;

        Destroy(gameObject);
        Destroy(target);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Player") return;

        other.GetComponent<PlayerRole>().SecondaryGun.Ammo += gBullets;
        other.GetComponent<PlayerRole>().PrimaryGun.Ammo += lBullets;
        Destroy(gameObject);
    }
}