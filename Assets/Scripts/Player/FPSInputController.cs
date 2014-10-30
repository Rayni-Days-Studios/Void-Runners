using UnityEngine;

public class FPSInputController : Photon.MonoBehaviour
{
    [AddComponentMenu("Character/FPS Input Controller")]

    private CharacterMotor _motor;

	// Use this for initialization
	void Awake ()
	{
        if (!photonView.isMine)
        {
            //We aren't the photonView owner, disable this script
            //RPC's and OnPhotonSerializeView will STILL get trough but we prevent Update from running
            enabled = false;
        }
	    _motor = GetComponent<CharacterMotor>();
	}
	
	// Update is called once per frame
	void Update ()
	{
	    var directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

	    if (directionVector != Vector3.zero)
	    {
	        var directionLength = directionVector.magnitude;
	        directionVector = directionVector/directionLength;

	        directionLength = Mathf.Min(1, directionLength);

	        directionLength = directionLength*directionLength;

	        directionVector = directionLength*directionVector;
	    }

	    _motor.InputMoveDirection = transform.rotation*directionVector;
	    _motor._inputJump = Input.GetButton("Jump");
	}
}
