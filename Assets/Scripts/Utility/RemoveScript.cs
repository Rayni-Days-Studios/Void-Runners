using System.Linq;
using UnityEngine;

public class RemoveScript : MonoBehaviour 
{
    public string[] collisionObject;
    public string triggerObject;
    public float removeTime;
    public bool removeAfterTime;
    public float remountOfTime = 1;
    public bool instantToggle;
    public string instantRemoval;

    void Update()
    {
        remountOfTime -= Time.deltaTime;
        removeTime -= Time.deltaTime;

        if (removeAfterTime && remountOfTime <= 0)
        {
            ObjectDestroy();
        }
    }

    //Remove object on collision
    void OnCollisionEnter (Collision other)
    {
        foreach (string str in collisionObject.Where(str => other.gameObject.tag == str))
        {
            if (removeTime <= 0)
            {
                ObjectDestroy();
            }
        }

        if (!instantToggle) return;
            if (other.gameObject.tag == instantRemoval)
            {
                ObjectDestroy();
            }
    }

    //Remove object on trigger
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == triggerObject && removeTime <= 0)
        {
            ObjectDestroy();
        }
    }

    void ObjectDestroy()
    {
        PhotonView pv = GetComponent<PhotonView>();

        if (pv != null && pv.instantiationId != 0)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}