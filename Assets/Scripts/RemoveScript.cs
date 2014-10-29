using System.Linq;
using UnityEngine;

public class RemoveScript : MonoBehaviour 
{
    public string[] CollisionObject;
    public string TriggerObject;
    public float RemoveTime;
    public bool RemoveAfterTime;
    public float AmountOfTime;
    public bool InstantToggle;
    public string InstantRemoval;

    void Update()
    {
        if (RemoveAfterTime)
        {
            Destroy(gameObject, AmountOfTime);
        }
    }

	//Remove object on collision
	void OnCollisionEnter (Collision other)
	{
	    foreach (var str in CollisionObject.Where(str => other.gameObject.tag == str))
	    {
	        Destroy(gameObject, RemoveTime);
	    }

	    if (!InstantToggle) return;
	        if (other.gameObject.tag == InstantRemoval)
	        {
	            Destroy(gameObject);
	        }
	}

    //Remove object on trigger
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == TriggerObject)
        {
            Destroy(gameObject, RemoveTime);
        }
    }
}
