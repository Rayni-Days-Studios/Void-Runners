using UnityEngine;

namespace Player
{
    public class FPSInputController : Photon.MonoBehaviour
    {
        private CharacterMotor _motor;

        // Use this for initialization
        void Awake ()
        {
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
            _motor.InputJump = Input.GetButton("Jump");
        }
    }
}