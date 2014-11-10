using UnityEngine;

public class FXManager : MonoBehaviour
{
	public GameObject BulletPrefab;

    [RPC]
    private void BulletFX(Vector3 startPos, Vector3 endPos)
    {
        Debug.Log("BulletFX");

        if (BulletPrefab != null)
        {
            GameObject bulletFX =
                (GameObject) Instantiate(BulletPrefab, startPos, Quaternion.LookRotation(endPos - startPos));

            LineRenderer lr = bulletFX.transform.Find("LineFX").GetComponent<LineRenderer>();

            if (lr != null)
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