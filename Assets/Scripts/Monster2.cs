using UnityEngine;
using System.Collections;

public class Monster2 : MonoBehaviour {

	public GameObject player;
	public float speed;
	bool collision = false;

	// Use this for initialization
	void Start () {
		animation.Play ("idle");
	}

	void OnTriggerEnter(Collider other) {
		collision = true;
		animation ["run"].wrapMode = WrapMode.Loop;
		animation.Play ("run"); 
	}

	void Update() {
		if (collision == true) {
			//Vector3.Lerp (transform.position, player.transform.position, speed);
			Quaternion rotation = Quaternion.LookRotation(player.transform.position - transform.position);
			transform.position += rotation * Vector3.forward * Time.deltaTime * speed;
		}		
	}
}
