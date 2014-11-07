using UnityEngine;

public class PlayerRole : MonoBehaviour
{
    public Rigidbody PrimaryBulletPrefab;
    public Rigidbody SecondaryBulletPrefab;
    public Transform BarrelEnd;
    public Gun PrimaryGun = new Gun(10, 10, 30, PrimaryBulletPrefab);
    public Gun SecondaryGun = new Gun(5, 5, 10, SecondaryBulletPrefab);
	
	// Update is called once per frame
	void Update () 
    {
        // If left click
        if (Input.GetButtonDown("Fire1"))
        {
            SecondaryGun.Shoot(BarrelEnd);
        }

        // If right click
        if (Input.GetButtonDown("Fire2"))
        {
            PrimaryGun.Shoot(BarrelEnd);
        }

        // If pressing R
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (PrimaryGun.LoadedAmmo/PrimaryGun.AmmoCap >= SecondaryGun.LoadedAmmo/SecondaryGun.AmmoCap)
            {
                PrimaryGun.Reload();
            }
            else
            {
                SecondaryGun.Reload();
            }
        }
	}
}
