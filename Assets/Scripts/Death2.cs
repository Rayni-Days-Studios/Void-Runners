using UnityEngine;
using System.Collections;

public class Death2 : MonoBehaviour {

	public Light redLight;
	public AudioSource activation;
	public AudioSource deathSound;
    public float speed;
    private GameObject player;

	bool dead;
	bool audioPlayed;
	bool audioPlayed2;
	bool collision = false;

	// Use this for initialization
	void Start () 
    {
        player = GameObject.FindGameObjectWithTag("Player");
		redLight.enabled = false;
		animation ["idle"].wrapMode = WrapMode.Loop;
		animation.Play ("idle");
	}
	
	void OnTriggerEnter (Collider other) 
    {
		if (!dead){
            if (other.gameObject.tag == "Bullet")
            {
                collision = true;
                gameObject.collider.enabled = false;
                if (audioPlayed2 == false)
                {
                    audioPlayed2 = true;
                    activation.Play();
                }
                redLight.enabled = true;
                animation["run"].wrapMode = WrapMode.Loop;
                animation.Play("run");
            }
		}
	}

    void OnCollisisonEnter(Collision other)
    {
        if (other.gameObject.tag == "Bullet2")
        {
            if (audioPlayed == false)
            {
                deathSound.Play();
                audioPlayed = true;
            }
            Destroy(other.gameObject);
            animation.Play("death");
            dead = true;
            collision = false;
            Destroy(gameObject, 1.2f);
        }
    }
	
	void Update() 
    {
		if (collision == true) 
        {
            transform.LookAt(player.transform);
			Quaternion rotation = Quaternion.LookRotation(player.transform.position - transform.position);
			transform.position += rotation * Vector3.forward * Time.deltaTime * speed;
		}		
	}
}