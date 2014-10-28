using UnityEngine;
using System.Collections;

public class MoveScript : MonoBehaviour {
	public int speed = 5;
	// Update is called once per frame
	void Update () {
		transform.Translate (new Vector3 (0, 0, 1) * Time.deltaTime * speed);
	}
}
