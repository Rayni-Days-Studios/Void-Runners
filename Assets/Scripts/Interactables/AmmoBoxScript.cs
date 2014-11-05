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

        GameObject.Find("_GameManager").GetComponent<PlayerRole>().SecondaryGun.LoadedAmmo += gBullets;
        GameObject.Find("_GameManager").GetComponent<PlayerRole>().PrimaryGun.LoadedAmmo += lBullets;
        Destroy(gameObject);
    }
}