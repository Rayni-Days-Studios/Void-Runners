using UnityEngine;

public class Gun {

    public int LoadedAmmo;
    public int AmmoClip;                    // Amount of bullets not currently in gun
    public int AmmoCap;                     // LoadedAmmo capacity in the gun
    public float Force;
    public Rigidbody BulletPrefab;
    public AudioSource ShootSound;

    public void Shoot(Transform spawnPoint)
    {
        // Only shoot if theres at least one bullet left
        if (LoadedAmmo > 0)
        {
            LoadedAmmo -= 1;
            ShootSound.Play();
            // Bullets are handled as RigidBodies as of right now.
            Rigidbody bulletInstance = Object.Instantiate(BulletPrefab, spawnPoint.position, spawnPoint.rotation) as Rigidbody;
            if (bulletInstance != null) bulletInstance.AddForce(spawnPoint.forward*Time.deltaTime*Force*1000f);
        }
    }

    public void Reload()
    {
        // If there is more outside the gun than missing inside.
        if (AmmoClip > AmmoCap-LoadedAmmo)
        {
            AmmoClip -= AmmoCap - LoadedAmmo;
            LoadedAmmo += AmmoCap - LoadedAmmo;
        }
        // There's still LoadedAmmo outside the gun, but not enough to fill it up
        else if (AmmoClip > 1 && AmmoClip < AmmoCap-LoadedAmmo)
        {
            // We know that AmmoClip can't fill LoadedAmmo, so we don't do a check here.
            LoadedAmmo += AmmoClip;
            AmmoClip = 0;
        }
        else if(AmmoClip < 1)
        {
            // Theres no more clips left, so we can't reload
            Debug.Log("No more clips");
        }
    }

    public Gun(int loadedAmmo, int ammoCap, int ammoClip, Rigidbody bulletPrefab)
    {
        LoadedAmmo = loadedAmmo;
        AmmoCap = ammoCap;
        AmmoClip = ammoClip;
        BulletPrefab = bulletPrefab;
    }

    public Gun()
    {
        LoadedAmmo = 0;
        AmmoCap = 0;
        AmmoClip = 0;
    }
}
