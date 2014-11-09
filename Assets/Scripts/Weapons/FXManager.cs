using UnityEngine;
using System.Collections;

public class FXManager : MonoBehaviour
{
    public AudioClip ShootAudioFX;
	public GameObject BulletPrefab;

	[RPC]
	void BulletFX( Vector3 startPos, Vector3 endPos ) 
    {
		Debug.Log ("SniperBulletFX");

        AudioSource.PlayClipAtPoint(ShootAudioFX, startPos);

		if(BulletPrefab != null) 
        {
			GameObject bulletFX = (GameObject)Instantiate(BulletPrefab, startPos, Quaternion.LookRotation(endPos - startPos));

			LineRenderer lr = bulletFX.transform.Find("LineFX").GetComponent<LineRenderer>();
			
            if(lr != null) 
            {
				lr.SetPosition(0, startPos);
				lr.SetPosition(1, endPos);
			}
			else 
            {
				Debug.LogError("BulletFXPrefab's linerenderer is missing.");
			}
		}
		else 
        {
			Debug.LogError("BulletFXPrefab is missing!");
		}

	}

}
