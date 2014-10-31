using UnityEngine;
using MonoBehaviour = Photon.MonoBehaviour;

public class NetworkCharacter : MonoBehaviour
{

    private Vector3 _realPosition;
    private Quaternion _realRotation;

    void Update()
    {
        if (photonView.isMine)
        {

        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, _realPosition, 0.1f);
            transform.rotation = Quaternion.Lerp(transform.rotation, _realRotation, 0.1f);

        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // Executed on the owner of this PhotonView; 
            // The server sends it's position over the network
            // "Encode" it, and send it

            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);

        }
        else
        {
            // Executed on the others; 
            // receive a position and set the object to it
            _realPosition = (Vector3)stream.ReceiveNext();
            _realRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}