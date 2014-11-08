using UnityEngine;

public class Player : MonoBehaviour {

    // Health
    public float MaxHitPoints = 100;
    private float currentHitPoints;

    public Transform BarrelEnd;
    public bool UseLightGun; // True = LightGun, False = BulletGun
    public Gun LightGun;
    public Gun BulletGun;

	// Use this for initialization
	void Start ()
	{
	    currentHitPoints = MaxHitPoints;

        if(UseLightGun)
            LightGun = new Gun();
        else
            BulletGun = new Gun();
	}
	
	// Update is called once per frame
	void Update () 
    {
        //If left click
        if (Input.GetButtonDown("Fire1"))
        {
            if (UseLightGun && LightGun.LoadedAmmo > 0)
                LightGun.Shoot(BarrelEnd);
            else
                print("No more energy left, reload!");

            if (!UseLightGun && BulletGun.LoadedAmmo > 0)
                BulletGun.Shoot(BarrelEnd);
            else
                print("No more bullets, reload!");
        }

        //If pressing R
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (UseLightGun)
                LightGun.Reload();
            else
                BulletGun.Reload();
        }
	}

    [RPC]
    public void TakeDamage(float amt)
    {
        currentHitPoints -= amt;

        if (currentHitPoints <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (GetComponent<PhotonView>().instantiationId == 0)
            Destroy(gameObject);
        else
            if (GetComponent<PhotonView>().isMine)
                PhotonNetwork.Destroy(gameObject);
    }
}
