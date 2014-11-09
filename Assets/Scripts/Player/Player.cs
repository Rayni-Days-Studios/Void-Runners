using UnityEngine;

public class Player : MonoBehaviour 
{
    private float cooldown;
    public bool UseLightGun; // True = LightGun, False = BulletGun
    public Gun LightGun;
    public Gun BulletGun;
    private FXManager fxManager;

	// Use this for initialization
	void Start ()
	{
	    fxManager = FindObjectOfType<FXManager>();
	    if (UseLightGun)
	        cooldown = LightGun.FireRate;
	    else
	        cooldown = BulletGun.FireRate;
	}
	
	// Update is called once per frame
	void Update () 
    {
        cooldown -= Time.deltaTime;
        //If left click
        if (Input.GetButton("Fire1"))
        {
            if (cooldown > 0) return;
                if (UseLightGun)
                    cooldown = LightGun.FireRate;
                else
                    cooldown = BulletGun.FireRate;
                if (UseLightGun && LightGun.LoadedAmmo > 0)
                    LightGun.Fire(fxManager);
                else
                    print("No more energy left, reload!");

                if (!UseLightGun && BulletGun.LoadedAmmo > 0)
                    BulletGun.Fire(fxManager);
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
}
