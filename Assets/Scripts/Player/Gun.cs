using UnityEngine;

public class Gun {

    public int Ammo;
    public int AmmoClip;             //Amount of bullets not currently in gun
    public int AmmoCap;                 //Ammo capacity
    public float Force;
    public Rigidbody BulletPrefab;
    public GameObject Bullet;
    public AudioSource ShootSound;

    public void Shoot(Transform spawnPoint)
    {
        if (Ammo > 0)
        {
            Ammo -= 1;
            ShootSound.Play();
            //Shoot
            Rigidbody bulletInstance =
                Object.Instantiate(BulletPrefab, spawnPoint.position, spawnPoint.rotation) as Rigidbody;
            if (bulletInstance != null) bulletInstance.AddForce(spawnPoint.forward*Time.deltaTime*Force*1000f);
        }
    }

    public void Reload()
    {
        //If there is more than 1 in clip.
        if (AmmoClip > 1)
        {
            AmmoClip -= AmmoCap - Ammo;
            Ammo += AmmoCap - Ammo;
        }
        else
        {
            Ammo += AmmoClip;
            AmmoClip = 0;
            Debug.Log("No more clips");
        }
    }

    public Gun(int ammo, int ammoCap, int ammoClip)
    {
        Ammo = ammo;
        AmmoCap = ammoCap;
        AmmoClip = ammoClip;
    }

    public Gun()
    {
        Ammo = 0;
        AmmoCap = 0;
        AmmoClip = 0;
    }
}
