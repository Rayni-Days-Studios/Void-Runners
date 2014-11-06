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
        public int Ammo;
        public int ammoClip;             //Amount of bullets not currently in gun
        public int ammoCap;                 //Ammo capacity
        public float Force;
        public Rigidbody BulletPrefab;
        public GameObject Bullet;
        public AudioSource ShootSound;

        public void Shoot(Transform spawnPoint)
        {
            Ammo -= 1;
            ShootSound.Play();
            //Shoot
            Rigidbody bulletInstance =
                Instantiate(BulletPrefab, spawnPoint.position, spawnPoint.rotation) as Rigidbody;
            if (bulletInstance != null) bulletInstance.AddForce(spawnPoint.forward*Time.deltaTime*Force*1000f);
        }

        public void Reload()
        {
            //If there is more than 1 in clip.
            if (ammoClip > 1)
            {
                ammoClip -= ammoCap - Ammo;
                Ammo += ammoCap - Ammo;
            }
            else
            {
                Ammo += ammoClip;
                ammoClip = 0;
                Debug.Log("No more clips");
            }
        }
    }

    void Update () 
    {
        //If left click
        if (Input.GetButtonDown("Fire1") && bulletGun.Ammo > 0)
        {
            bulletGun.Shoot(barrelEnd);
        }
        else
        {
            Debug.Log("No more bullets, reload!");
        }
        //If right click
        if(Input.GetButtonDown("Fire2") && lightGun.Ammo > 0)
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