using UnityEngine;
using System.Collections;

public class SpawnPoint : MonoBehaviour {

    public int gAmmo;
    public int lAmmo;
	public float gForce;
    public float lForce;
    public Rigidbody gBulletPrefab;
    public Rigidbody lBulletPrefab;
    public Transform barrelEnd;
	public GameObject gBullet;
	public GameObject lBullet;
	public AudioSource shoot1;
	public AudioSource shoot2;

	void Update () 
	{
        if (Input.GetButtonDown("Fire1") && gAmmo > 0)
        {
            gAmmo -= 1;
            shoot1.Play();
            Rigidbody gBulletInstance;
            gBulletInstance = Instantiate(gBulletPrefab, barrelEnd.position, barrelEnd.rotation) as Rigidbody;
            gBulletInstance.AddForce(barrelEnd.forward * Time.deltaTime * gForce * 1000f);
        }

		if(Input.GetButtonDown("Fire2") && lAmmo > 0)
		{
            lAmmo -= 1;
			shoot2.Play();
            Rigidbody lBulletInstance;
			lBulletInstance = Instantiate(lBulletPrefab, barrelEnd.position, barrelEnd.rotation) as Rigidbody;
			lBulletInstance.AddForce(barrelEnd.forward * Time.deltaTime * lForce * 1000f);
        }
	}
}