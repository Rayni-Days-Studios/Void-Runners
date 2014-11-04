using System.Linq;
using UnityEngine;

public class Death : MonoBehaviour
{
    public TriggerScript Trigger;

    public Light redLight;
    public AudioSource activation;
    public AudioSource deathSound;
    public float speed;
    private static GameObject player;
    private KillPlayer killPlayerRef;


    private bool dead;
    private bool audioPlayed;
    private bool audioPlayed2;
    private bool collision;

    // Use this for initialization
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        killPlayerRef = player.GetComponent<KillPlayer>();
        redLight.enabled = false;
        animation["idle"].wrapMode = WrapMode.Loop;
        animation.Play("idle");
    }

    void Update()
    {
        if (Trigger != null)
        {
            if (Trigger.list.Any(obj => obj != null && obj.gameObject.tag == "Bullet"))
            {
                collision = true;
                Destroy(Trigger.gameObject);
                Trigger = null;

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
        if (collision != true) return;
        if (player.gameObject == null) return;
        transform.LookAt(player.transform);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        rigidbody.velocity = transform.forward * speed;
    }

    void OnCollisionEnter(Collision other)
    {
        if (dead) return;

        if (other.gameObject.tag == "Bullet2")
        {
            if (!audioPlayed)
            {
                deathSound.Play();
                audioPlayed = true;
            }
            Destroy(other.gameObject);
            animation.Play("death");
            collision = false;
            dead = true;
            Destroy(gameObject, 1.2f);
        }
        if (other.gameObject.tag == "Player")
        {
            killPlayerRef.Die(transform.position + Vector3.up*1.8f, 0.75f, 1);
            animation.Blend("attack01", 8f, 0.8f);
        }
        if (player.gameObject != null) return;
            Application.LoadLevel(Application.loadedLevel);
    }
}