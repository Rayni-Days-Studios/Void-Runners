using UnityEngine;
using System.Collections;

public class RemoveBullet : MonoBehaviour {
    public float removeTime;
	void OnCollisionEnter (Collision other) 
    {
        if (other.gameObject.tag == "Walls")
        {
            Destroy(gameObject, removeTime);
        }
	}
}
