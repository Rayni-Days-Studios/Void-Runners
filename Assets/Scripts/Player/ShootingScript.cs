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
        public float Force;
        public Rigidbody BulletPrefab;
        public GameObject Bullet;
        public AudioSource ShootSound;

        public void Shoot(Transform spawnPoint)
        {
            Ammo -= 1;
            ShootSound.Play();
            //Shoot
            Rigidbody bulletInstance = Instantiate(BulletPrefab, spawnPoint.position, spawnPoint.rotation) as Rigidbody;
            if (bulletInstance != null) bulletInstance.AddForce(spawnPoint.forward * Time.deltaTime * Force * 1000f);
        }
    }

    void Update () 
    {
        //If left click
        if (Input.GetButtonDown("Fire1") && bulletGun.Ammo > 0)
        {
            bulletGun.Shoot(barrelEnd);
        }
        //If right click
        if(Input.GetButtonDown("Fire2") && lightGun.Ammo > 0)
        {
            lightGun.Shoot(barrelEnd);
        }
    }
}