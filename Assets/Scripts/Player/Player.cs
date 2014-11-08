using UnityEngine;

public class Player : MonoBehaviour {

    public Transform BarrelEnd;
    public bool UsePrimaryGun; // True = LightGun, False = BulletGun
    public Gun LightGun;
    public Gun BulletGun;

	// Use this for initialization
	void Start () 
    {
        LightGun = new Gun();
        BulletGun = new Gun();
	}
	
	// Update is called once per frame
	void Update () 
    {
        //If left click
        if (Input.GetButtonDown("Fire1"))
        {
            if (UsePrimaryGun && LightGun.LoadedAmmo > 0)
                LightGun.Shoot(BarrelEnd);
            else
                print("No more energy left, reload!");

            if (!UsePrimaryGun && BulletGun.LoadedAmmo > 0)
                BulletGun.Shoot(BarrelEnd);
            else
                print("No more bullets, reload!");
        }

        //If pressing R
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (UsePrimaryGun)
                LightGun.Reload();
            else
                BulletGun.Reload();
        }
	}
}
