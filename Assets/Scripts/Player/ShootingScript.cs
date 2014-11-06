using System;
using UnityEngine;

public class ShootingScript : MonoBehaviour 
{
    public Transform barrelEnd;
    public Gun lightGun;
    public Gun bulletGun;

    [Serializable]
    public struct Gun
    {
        public int TotalAmmo;
        public int LoadedAmmo;             //Amount of bullets not currently in gun
        public int AmmoCap;                 //Ammo capacity
        public float Force;
        public Rigidbody BulletPrefab;
        public GameObject Bullet;
        public AudioSource ShootSound;

        public void Shoot(Transform spawnPoint)
        {
            LoadedAmmo -= 1;
            ShootSound.Play();
            //Shoot
            Rigidbody bulletInstance =
                Instantiate(BulletPrefab, spawnPoint.position, spawnPoint.rotation) as Rigidbody;
            if (bulletInstance != null) bulletInstance.AddForce(spawnPoint.forward*Time.deltaTime*Force*1000f);
        }


        public void Reload()
        {
            // If there is more outside the gun than missing inside.
            if (TotalAmmo > AmmoCap - LoadedAmmo)
            {
                TotalAmmo -= AmmoCap - LoadedAmmo;
                LoadedAmmo += AmmoCap - LoadedAmmo;
            }
            // There's still ammo outside the gun, but not enough to fill it up
            else if (TotalAmmo > 1 && TotalAmmo < AmmoCap - LoadedAmmo)
            {
                // We know that AmmoClip can't fill Ammo, so we don't do a check here.
                LoadedAmmo += TotalAmmo;
                TotalAmmo = 0;
            }
            else if(TotalAmmo < 1)
            {
                // Theres no more clips left, so we can't reload
                Debug.Log("Out of ammo");
            }
        }
    }

    void Update () 
    {
        //If left click
        if (Input.GetButtonDown("Fire1") && bulletGun.TotalAmmo > 0)
        {
            bulletGun.Shoot(barrelEnd);
        }
        else
        {
            Debug.Log("No more bullets, reload!");
        }
        //If right click
        if(Input.GetButtonDown("Fire2") && lightGun.TotalAmmo > 0)
        {
            lightGun.Shoot(barrelEnd);
        }

        //If pressing R
        if (Input.GetKeyDown(KeyCode.R))
        {
            bulletGun.Reload();
        }
    }
}