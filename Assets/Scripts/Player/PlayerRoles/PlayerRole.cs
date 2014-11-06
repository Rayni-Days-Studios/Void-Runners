using UnityEngine;

public class PlayerRole : MonoBehaviour {

    public Transform BarrelEnd;
    public Gun PrimaryGun = new Gun();
    public Gun SecondaryGun = new Gun();
	
	// Update is called once per frame
	void Update () 
    {
        //If left click
        if (Input.GetButtonDown("Fire1") && SecondaryGun.Ammo > 0)
        {
            SecondaryGun.Shoot(BarrelEnd);
        }
        else
        {
            Debug.Log("No more bullets, reload!");
        }
        //If right click
        if (Input.GetButtonDown("Fire2"))
        {
            PrimaryGun.Shoot(BarrelEnd);
        }

        //If pressing R
        if (Input.GetKeyDown(KeyCode.R))
        {
            PrimaryGun.Reload();
        }
	}
}
