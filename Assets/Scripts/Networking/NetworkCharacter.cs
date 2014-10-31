using UnityEngine;

namespace Assets.Scripts.Networking
{
    public class NetworkCharacter : Photon.MonoBehaviour
    {
        //Stores the actual position and rotation
        private Vector3 _realPosition;
        private Quaternion _realRotation;

        void Update()
        {
            if (photonView.isMine) return;
                //Lerps the character from the local position to their actual position
                //This smooths the local movement; reduces jitter
                transform.position = Vector3.Lerp(transform.position, _realPosition, 0.1f);
                transform.rotation = Quaternion.Lerp(transform.rotation, _realRotation, 0.1f);
        }

        void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                //Executed on the owner of this PhotonView; 
                //The server sends its position over the network
                //"Encode" it, and send it
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);

            }
            else
            {
                //This receives the actual location of the object
                _realPosition = (Vector3)stream.ReceiveNext();
                _realRotation = (Quaternion)stream.ReceiveNext();
            }
        }
    }
}