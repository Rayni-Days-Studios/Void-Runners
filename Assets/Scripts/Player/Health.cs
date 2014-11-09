using UnityEngine;

public class Health : MonoBehaviour
{
    public GUIStyle MyTextStyle;
    public float MaxHitPoints = 100;
    private float currentHitPoints;
    private GameObject cam;
    
    void Start ()
	{
	    currentHitPoints = MaxHitPoints;
        cam = GameObject.Find("StandbyCamera");
	}

    [RPC]
    public void TakeDamage(float amount)
    {
        currentHitPoints -= amount;

        if (currentHitPoints <= 0)
            Die();
    }

    void Die()
    {
        if (GetComponent<PhotonView>().instantiationId == 0)
            Destroy(gameObject);
        else if (GetComponent<PhotonView>().isMine)
        {
            PhotonNetwork.Destroy(gameObject);
            PhotonNetwork.Instantiate(cam.ToString(), cam.transform.position, cam.transform.rotation, 0);
            GUI.TextField(new Rect(0, 0, Screen.width/2, Screen.height/2), "Monster is dead. Team wins!", MyTextStyle);
        }
    }
}
