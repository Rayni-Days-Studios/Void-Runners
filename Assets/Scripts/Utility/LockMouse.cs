using UnityEngine;

public class LockMouse : MonoBehaviour {

    void Awake()
    {
        Screen.lockCursor = true;
    }

	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (Input.GetKeyDown(KeyCode.Escape))
	    {
	        Screen.lockCursor = !Screen.lockCursor;
	    }
	}
}
