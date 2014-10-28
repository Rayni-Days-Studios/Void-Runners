using UnityEngine;
using System.Collections;

public class PlayerDeath2 : MonoBehaviour {
	void OnTriggerEnter(Collider other){
		if(other.gameObject.tag == "Enemy"){
			Destroy (gameObject, 0.5f);
		}
	}
}
