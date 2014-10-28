using UnityEngine;

public class Death : MonoBehaviour 
{
    public TriggerScript Trigger;

	public Light RedLight;
	public AudioSource Activation;
	public AudioSource DeathSound;
    public float Speed;
    private GameObject _player;

    
	bool _dead;
	bool _audioPlayed;
	bool _audioPlayed2;
	bool _collision;

	// Use this for initialization
	void Start () 
    {
        _player = GameObject.FindGameObjectWithTag("Player");
		RedLight.enabled = false;
		animation ["idle"].wrapMode = WrapMode.Loop;
		animation.Play ("idle");
	}

    void Update()
    {
        if (Trigger != null)
        {
            foreach (var obj in Trigger.List)
            {
                if (obj == null || obj.gameObject.tag != "Bullet") continue;

                _collision = true;
                Destroy(Trigger.gameObject);
                Trigger = null;

                if (_audioPlayed2 == false)
                {
                    _audioPlayed2 = true;
                    Activation.Play();
                }
                RedLight.enabled = true;
                animation["run"].wrapMode = WrapMode.Loop;
                animation.Play("run");
                break;
            }
        }
        if (_collision != true) return;

        transform.LookAt(_player.transform);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        rigidbody.velocity = transform.forward * Speed;
    }

	void OnCollisionEnter (Collision other) 
    {
	    if (_dead) return;

	    if(other.gameObject.tag == "Bullet2")
	    {
	        if(!_audioPlayed)
	        {
	            DeathSound.Play();
	            _audioPlayed = true;
	        }
	        Destroy (other.gameObject);
	        animation.Play("death");
	        _collision = false;
	        _dead = true;
	        Destroy (gameObject, 1.2f);
	    }
	    if (other.gameObject.tag != "Player") return;

	    _player.GetComponent<KillPlayer>().Die(transform.position + Vector3.up * 1.8f, 0.75f, 1);
	    animation.Blend("attack01", 8f, 0.8f);
    }
}