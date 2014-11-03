using System;
using UnityEngine;

public class ShootingScript : MonoBehaviour 
{
    public Transform BarrelEnd;
    public Gun LightGun;
    public Gun gun;

    [Serializable]
    public struct Gun
    {
        public int Ammo;
        public float Force;
        public Rigidbody BulletPrefab;
        public GameObject Bullet;
        public AudioSource ShootSound;

        public void Shoot(Transform spawnPoint)
        {
            Ammo -= 1;
            ShootSound.Play();
            //Shoot
            var bulletInstance = Instantiate(BulletPrefab, spawnPoint.position, spawnPoint.rotation) as Rigidbody;
            if (bulletInstance != null) bulletInstance.AddForce(spawnPoint.forward * Time.deltaTime * Force * 1000f);
        }
    }

    void Update () 
    {
        //If left click
        if (Input.GetButtonDown("Fire1") && gun.Ammo > 0)
        {
            gun.Shoot(BarrelEnd);
        }
        //If right click
        if(Input.GetButtonDown("Fire2") && LightGun.Ammo > 0)
        {
            LightGun.Shoot(BarrelEnd);
        }
    }
}