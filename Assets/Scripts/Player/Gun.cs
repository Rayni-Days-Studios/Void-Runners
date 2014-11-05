using UnityEngine;

public class Gun {

    public int Ammo;
    public int AmmoClip;                    // Amount of bullets not currently in gun
    public int AmmoCap;                     // Ammo capacity in the gun
    public float Force;
    public Rigidbody BulletPrefab;
    public GameObject Bullet;
    public AudioSource ShootSound;

    public void Shoot(Transform spawnPoint)
    {
        // Only shoot if theres bullets left
        if (Ammo > 0)
        {
            Ammo -= 1;
            ShootSound.Play();
            // Bullets are handled as RigidBodies as of right now.
            Rigidbody bulletInstance = Object.Instantiate(BulletPrefab, spawnPoint.position, spawnPoint.rotation) as Rigidbody;
            if (bulletInstance != null) bulletInstance.AddForce(spawnPoint.forward*Time.deltaTime*Force*1000f);
        }
    }

    public void Reload()
    {
        // If there is more outside the gun than missing inside.
        if (AmmoClip > AmmoCap-Ammo)
        {
            AmmoClip -= AmmoCap - Ammo;
            Ammo += AmmoCap - Ammo;
        }
        // There's still ammo outside the gun, but not enough to fill it up
        else if (AmmoClip > 1 && AmmoClip < AmmoCap-Ammo)
        {
            // We know that AmmoClip can't fill Ammo, so we don't do a check here.
            Ammo += AmmoClip;
            AmmoClip = 0;
        }
        else if(AmmoClip < 1)
        {
            // Theres no more clips left, so we can't reload
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
