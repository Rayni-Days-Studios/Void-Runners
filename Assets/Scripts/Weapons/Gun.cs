using System;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable]
public class Gun 
{
    public int TotalAmmo;
    public int LoadedAmmo;             //Amount of bullets not currently in gun
    public int AmmoCap;                 //Ammo capacity
    public float Damage;
    public float FireRate;
    public Transform spawnPoint;
    public GameObject Bullet;

    public void Fire(FXManager fxManager)
    {
        LoadedAmmo -= 1;
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        Vector3 hitPoint;
        Transform hitTransform = FindClosestHitObject(ray, out hitPoint);

        if (hitTransform != null)
        {
            Debug.Log("We hit: " + hitTransform.name);

            // We could do a special effect at the hit location
            // DoRicochetEffectAt( hitPoint );

            Health healthScript = hitTransform.GetComponent<Health>();

            while (healthScript == null && hitTransform.parent)
            {
                hitTransform = hitTransform.parent;
                healthScript = hitTransform.GetComponent<Health>();
            }

            // Once we reach here, hitTransform may not be the hitTransform we started with!

            if (healthScript != null)
            {
                // This next line is the equivalent of calling:
                //    				h.TakeDamage( damage );
                // Except more "networky"
                PhotonView pView = healthScript.GetComponent<PhotonView>();
                if (pView == null)
                    Debug.LogError("PhotonView not found on object");
                else
                {
                    healthScript.GetComponent<PhotonView>().RPC("TakeDamage", PhotonTargets.AllBuffered, Damage);
                    Debug.Log("TakeDAMAGE YO");
                }

            }

            if (fxManager != null)
                DoGunFX(hitPoint, fxManager);
        }
        else
            // We didn't hit anything (except empty space), but let's do a visual FX anyway
            if (fxManager != null)
            {
                hitPoint = Camera.main.transform.position + (Camera.main.transform.forward * 100f);
                DoGunFX(hitPoint,fxManager);
            }
    }

    void DoGunFX(Vector3 hitPoint, FXManager fxManager)
    {
        fxManager.GetComponent<PhotonView>().RPC("BulletFX", PhotonTargets.All, spawnPoint.position, hitPoint);
    }


    Transform FindClosestHitObject(Ray ray, out Vector3 hitPoint)
    {

        RaycastHit[] hits = Physics.RaycastAll(ray);

        Transform closestHit = null;
        float distance = 0;
        hitPoint = Vector3.zero;

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform != spawnPoint && (closestHit == null || hit.distance < distance))
            {
                // We have hit something that is:
                // a) not us
                // b) the first thing we hit (that is not us)
                // c) or, if not b, is at least closer than the previous closest thing

                closestHit = hit.transform;
                distance = hit.distance;
                hitPoint = hit.point;
            }
        }

        // closestHit is now either still null (i.e. we hit nothing) OR it contains the closest thing that is a valid thing to hit
        return closestHit;
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
        else if (TotalAmmo < 1)
        {
            // Theres no more clips left, so we can't reload
            Debug.Log("Out of ammo");
        }
    }
}