using System.Linq;
using UnityEngine;

public class RemoveScript : MonoBehaviour 
{
    public string[] collisionObject;
    public string triggerObject;
    public float removeTime;
    public bool removeAfterTime;
    public float remountOfTime;
    public bool instantToggle;
    public string instantRemoval;

    void Update()
    {
        if (removeAfterTime)
        {
            Destroy(gameObject, remountOfTime);
        }
    }

    //Remove object on collision
    void OnCollisionEnter (Collision other)
    {
        foreach (string str in collisionObject.Where(str => other.gameObject.tag == str))
        {
            Destroy(gameObject, removeTime);
        }

        if (!instantToggle) return;
            if (other.gameObject.tag == instantRemoval)
            {
                Destroy(gameObject);
            }
    }

    //Remove object on trigger
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == triggerObject)
        {
            Destroy(gameObject, removeTime);
        }
    }
}